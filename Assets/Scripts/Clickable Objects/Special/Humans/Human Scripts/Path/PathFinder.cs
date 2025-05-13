using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;


public static class PathFinder
{
    #region Struct
    /// <summary>Holds info about elevators and new interests.</summary>
    struct SearchCoords
    {
        /// <summary>Entry points to possible interests, coresponds to <b>entrypoints</b> in <see cref="Prep(GridPos, List{ClickableObject})"/></summary>
        public List<GridPos> entryPoints;
        /// <summary>Entry points to elevators.</summary>
        public List<GridPos> elevEnterPositions;
        /// <summary>Elevator anchors.</summary>
        public List<GridPos> elevPositions;
        /// <summary>Elevator connections, which are purged after one use.</summary>
        public List<List<int>> connections;

        /// <summary>
        /// Assigns interests.
        /// </summary>
        /// <param name="gridPos"></param>
        public SearchCoords(GridPos gridPos)
        {
            entryPoints = new() { gridPos };
            elevEnterPositions = new();
            elevPositions = new();
            connections = new();
        }
    }
    #endregion

    #region Variables
    const float ROAD_COST = 1f;
    const float FASTROUTE_COST = 0.75f;
    const float ELEVATOR_COST = 2f;
    #endregion

    #region Humans
    /// <summary>
    /// Access point for <see cref="Human"/> paths. <br/>
    /// </summary>
    /// <param name="objects">Possible interests.</param>
    /// <param name="h">Human that needs the path.</param>
    /// <returns>New path with an interest (the closest object from <paramref name="objects"/>).</returns>
    public static JobData FindPath(List<ClickableObject> objects, Human h)
    {
        if (objects.Count == 0)
            return new();
        GridPos _start = h.GetPos();
        Plan plan = Prep(_start, objects).Result;
        if (plan.index > -1)
        {
            ClickableObject interest = objects[plan.index];
            Building b = interest.GetComponent<Building>();
            if (b)
            {
                Pipe p = interest.GetComponent<Pipe>();
                if (p)
                {
                    plan.path.RemoveAt(plan.path.Count - 1);
                }
                else if (MyGrid.GetGridItem(_start).id != b.id && plan.foundNormaly)
                    plan.path.Add(BuildingStep(plan.path.Count > 0 ? plan.path[^1] : _start, b.gameObject, 1));
            }
            else
            {
                Rock rock = interest.GetComponent<Rock>();
                if (rock)
                {
                    plan.path.RemoveAt(plan.path.Count - 1);
                }
            }
            return new(_path: plan.path, _interest: interest);
        }
        return new JobData(_path: new(), _interest: null);
    }
    /// <summary>
    /// Scrapes positions from <paramref name="objects"/>. <br/>
    /// Then calls <see cref="LookForPath(GridPos, Building, SearchCoords, Plan, Type)"/>.
    /// </summary>
    /// <param name="_start"></param>
    /// <param name="objects"></param>
    /// <returns>Path to the object and index of the object.</returns>
    async static Task<Plan> Prep(GridPos _start, List<ClickableObject> objects)
    {
        List<int> entryPoints = new();
        SearchCoords coordinates = new();
        coordinates.entryPoints = new();
        coordinates.elevEnterPositions = new();
        coordinates.elevPositions = new();
        coordinates.connections = new();

        Building part = MyGrid.GetGridItem(_start).GetComponent<Building>(); // gets tile build reference if standing on it
        Plan plan = new();

        for (int i = 0; i < objects.Count; i++)
        {
            Building building = null;
            try
            {
                building = objects[i].GetComponent<Building>();
            }
            catch (Exception e)
            {
                Debug.Log("object is null" + e);
            }
            if (building)
            {
                GridPos gp = building.GetPos();
                /*Pipe pipe = building as Pipe;
                if (pipe)
                {
                    coordinates.positions.Add(new(pipe.transform.position));
                    entryPoints.Add(i);
                    continue;
                }*/

                foreach (RectTransform t in MyGrid.GetOverlay(gp.y).buildingOverlays.First(q => q.name == building.id.ToString())
                    .GetComponentsInChildren<Image>().Select(q => q.transform))
                {
                    coordinates.entryPoints.Add(new(Mathf.Floor(t.position.x), gp.y, Mathf.Floor(t.position.z)));
                    entryPoints.Add(i);
                }
                if (part != null && part.id == building.id) // the build that the worker is standing on, is one of the destinations 
                {
                    plan.path = new();
                    plan.index = i;
                    return plan;
                }
            }
            else
            {
                coordinates.entryPoints.Add(objects[i].GetPos());
                entryPoints.Add(i);
            }
        }

        // prep for elevators
        foreach (Building el in MyGrid.GetBuildings(q => q is Elevator))
        {
            GridPos gp = el.GetPos();
            foreach (RectTransform t in MyGrid.GetOverlay(gp.y).buildingOverlays.First(q => q.name == el.id.ToString()).GetComponentsInChildren<Image>().Select(q => q.transform))//item in building.blueprint.itemList.Where(q=> q.itemType == GridItemType.Entrance).Skip(1))
            {
                coordinates.elevPositions.Add(gp);
                coordinates.elevEnterPositions.Add(new(Mathf.Floor(t.position.x), gp.y, Mathf.Floor(t.position.z)));
                coordinates.connections.Add(new());
            }
            for (int i = 0; i < 5; i++)
            {
                if (MyGrid.GetGridItem(new(gp.x, i, gp.z)) is Elevator)
                {
                    coordinates.connections[^1].Add(i);
                }
            }
        }

        await LookForPath(_start, part, coordinates, plan, typeof(Road));
        if (plan.index > -1)
        {
            if (!part)
            {
                plan.path.RemoveAt(0);
            }
            plan.index = entryPoints[plan.index];
        }
        return plan;
    }

    #endregion Humans

    #region Pipes
    /// <summary>
    /// Finds shortest path connecting <paramref name="startPos"/> and <paramref name="activePos"/>.
    /// </summary>
    /// <param name="startPos">Path start position.</param>
    /// <param name="activePos">Path end position.</param>
    /// <param name="enterObjectType">Object filter, null means anything.</param>
    /// <returns>path</returns>
    public static List<GridPos> FindPath(GridPos startPos, GridPos activePos, Type enterObjectType)
    {
        Plan p = new();
        if (!startPos.Equals(activePos))
            LookForPath(startPos, null, new(activePos), p, enterObjectType);
        return p.path;
    }
    #endregion

    #region Looping
    /// <summary>
    /// Starts really searching for the path.
    /// </summary>
    /// <param name="_start">Starting position.</param>
    /// <param name="buildingTile">Building on <paramref name="_start"/> position.</param>
    /// <param name="searchCoords">Search Data.</param>
    /// <param name="plan">Result</param>
    /// <param name="enterObjectType">Object filter, null means anything.</param>
    /// <returns></returns>
    static Task LookForPath(GridPos _start, Building buildingTile, SearchCoords searchCoords, Plan plan, Type enterObjectType)
    {
        // Move if starting inside a building
        if (buildingTile != null)
            _start = BuildingStep(_start, buildingTile.gameObject, -1);

        Queue queue = new(_start);
        // Check if you already arrived, or try to access different levels
        if (!Check(new(_start, 0, null), searchCoords, plan, queue))
        {
            return Task.CompletedTask;
        }
        PathNode prevNode;
        while ((prevNode = queue.Dequeue()) != null)
        {
            try
            {
                for (int i = 0; i < 4; i++) // checks in every direction
                {
                    PathNode checkNode = new(i, prevNode);
                    ClickableObject clickable = MyGrid.GetGridItem(checkNode.pos);
                    if (clickable)
                    {
                        if (Check(checkNode, searchCoords, plan, queue))
                        {
                            if (CanEnter(clickable, enterObjectType))
                            {
                                checkNode.minCost += ROAD_COST;
                                queue.Enqueue(checkNode);
                            }
                        }
                        else
                        {
                            return Task.CompletedTask;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("haaha it's null you dumbass: " + "\n" + e); // log error
            }
        }
        return Task.CompletedTask;
    }

    #endregion Looping

    #region Tile Checks
    /// <summary>
    /// Checks if the Tile can be accesed.
    /// </summary>
    /// <param name="clickable">tile</param>
    /// <param name="t">requested type</param>
    /// <returns></returns>
    static bool CanEnter(ClickableObject clickable, Type t)
    {
        return clickable.GetType() == t;
    }

    /// <summary>
    /// Goes through all entrypoints in <paramref name="searchCoords"/> and tries to end the search.
    /// </summary>
    /// <param name="checkNode">Node to check.</param>
    /// <param name="searchCoords">Search Data</param>
    /// <param name="plan">Result</param>
    /// <param name="queue">Queue for efficient search.</param>
    /// <param name="firstPass">Was called recursivly?</param>
    /// <returns>True to continue, false to end search.</returns>
    static bool Check(PathNode checkNode, SearchCoords searchCoords, Plan plan, Queue queue, bool firstPass = true)
    {
        int id = -1;
        int count = 0;
        foreach (GridPos pos in searchCoords.entryPoints)
        {
            if (checkNode.pos.Equals(pos))
            {
                id = count;
                break;
            }
            count++;
        }
        if (id > -1) // if there is an entry point or a job on the checkVec
        {
            List<GridPos> path = new();
            while (checkNode != null)
            {
                path.Add(checkNode.pos);
                checkNode = checkNode.previous;
            }
            path.Reverse();
            plan.path = path;
            plan.index = id;

            return false;
        }
        else
        {
            if (firstPass)
                return MoveToNewLevel(searchCoords, checkNode, plan, queue);
            return true;
        }
    }

    /// <summary>
    /// Tries to add new path nodes to different levels.
    /// </summary>
    /// <param name="searchCoords">Path Data</param>
    /// <param name="checkNode">Tile</param>
    /// <param name="plan">Result</param>
    /// <param name="queue">Search queue.</param>
    /// <returns></returns>
    static bool MoveToNewLevel(SearchCoords searchCoords, PathNode checkNode, Plan plan, Queue queue)
    {
        for (int i = 0; i < searchCoords.elevEnterPositions.Count; i++)
        {
            if (searchCoords.elevEnterPositions[i].Equals(checkNode.pos) && searchCoords.connections[i].Count > 1)
            {
                GridPos gp = searchCoords.elevPositions[i];
                PathNode inElevatorNode = new(gp, checkNode.minCost + 1, checkNode);
                for (int j = 0; j < searchCoords.connections[i].Count; j++)
                {
                    int level = searchCoords.connections[i][j];
                    if (level == checkNode.pos.y)
                        continue;
                    GridPos elevatorPos = new GridPos(gp.x, level, gp.z);
                    PathNode outElevatorNode = new(elevatorPos, inElevatorNode.minCost, inElevatorNode);
                    ClickableObject el = MyGrid.GetGridItem(elevatorPos);

                    foreach (RectTransform t in MyGrid.GetOverlay(level).buildingOverlays
                        .First(q => q.name == el.id.ToString())
                        .GetComponentsInChildren<Image>().Select(q => q.transform))//item in building.blueprint.itemList.Where(q=> q.itemType == GridItemType.Entrance).Skip(1))
                    {
                        PathNode finishMove =
                            new(new(Mathf.Round(t.transform.position.x), level, Mathf.Round(t.transform.position.z)), outElevatorNode.minCost + 1, outElevatorNode);
                        if (Check(finishMove, searchCoords, plan, queue, false))
                        {
                            queue.Enqueue(finishMove);
                        }
                        else
                        {
                            plan.path.RemoveAt(0);
                            plan.path.RemoveAt(0);
                            plan.path.RemoveAt(plan.path.Count - 1);
                            plan.foundNormaly = false;
                            return false;
                        }
                    }
                }
            }
        }
        return true;
    }
    #endregion Tile Checks

    /// <summary>
    /// Moves in/out of building with entrypoints. (else they would go though walls)
    /// </summary>
    /// <param name="_vec">position</param>
    /// <param name="building">building</param>
    /// <param name="mod">
    ///     1  = in <br/>
    ///     -1 = out
    /// </param>
    /// <returns>Calculated additional position.</returns>
    public static GridPos BuildingStep(GridPos _vec, GameObject building, int mod)
    {
        GridPos vec = new(_vec.x, _vec.y, _vec.z);
        float rotation = building.transform.eulerAngles.y;
        int rot = Mathf.RoundToInt(rotation / 90f);
        switch (rot)
        {
            case 0:
                vec.z += (1 * mod);
                break;
            case 1:
                vec.x += (1 * mod);
                break;
            case 2:
                vec.z -= (1 * mod);
                break;
            case 3:
                vec.x -= (1 * mod);
                break;
        }
        return vec;
    }

}