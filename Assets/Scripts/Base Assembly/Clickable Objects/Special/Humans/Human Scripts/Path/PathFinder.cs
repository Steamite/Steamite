

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
    public static JobData FindPath(List<ClickableObject> objects, Human h, int maxLength = -1)
    {
        if (objects.Count == 0)
            return JobData.CreateEmpty();
        GridPos _start = h.GetPos();
        Plan plan = Prep(_start, objects, maxLength).Result;
        if (plan.index > -1)
        {
            ClickableObject interest = objects[plan.index];
            if (interest is Building b && interest is not Pipe)
            {
                // creates last building step(uses start pos if there are no path nodes)
                if (MyGrid.GetGridItem(_start) != b && plan.foundNormaly)
                    plan.path.Add(BuildingStep(plan.path.Count > 0 ? plan.path[^1] : _start, b.gameObject, 1));
            }
            else if (interest is Rock rock)
            {
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
    async static Task<Plan> Prep(GridPos _start, List<ClickableObject> objects, int maxLength)
    {
        List<int> entryPoints = new();
        SearchCoords coordinates = new();
        coordinates.entryPoints = new();
        coordinates.elevEnterPositions = new();
        coordinates.elevPositions = new();
        coordinates.connections = new();

        Building part = MyGrid.GetGridItem(_start) as Building; // gets tile build reference if standing on it
        Plan plan = new();

        for (int i = 0; i < objects.Count; i++)
        {
            if (objects[i] is Building building)
            {
                GridPos gp = building.GetPos();
                Pipe pipe = building as Pipe;
                if (pipe)
                {
                    coordinates.entryPoints.Add(pipe.GetPos());
                    entryPoints.Add(i);
                    continue;
                }

                foreach (GridPos entryPos in building.entryPoints.EnabledTiles)
                {
                    if (entryPos.Equals(_start))
                    {
                        plan.path.Add(BuildingStep(entryPos, building.gameObject, 1));
                        plan.index = i;
                        plan.foundNormaly = false;
                        return plan;
                    }
                    else
                    {
                        coordinates.entryPoints.Add(entryPos);
                        entryPoints.Add(i);
                    }
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
            foreach (GridPos pos in el.entryPoints.EnabledTiles)//item in building.blueprint.itemList.Where(q=> q.itemType == GridItemType.Entrance).Skip(1))
            {
                // TODO, will work weird for larger elevators
                coordinates.elevPositions.Add(gp);
                coordinates.elevEnterPositions.Add(pos);
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

        await LookForPath(_start, part, coordinates, plan, typeof(Road), maxLength);
        if (plan.index > -1)
        {/*
            if (!part)
            {
                plan.path.RemoveAt(0);
            }*/
            plan.index = entryPoints[plan.index];

            for (int i = plan.path.LastIndexOf(_start); i >= 0; i--)
            {
                plan.path.RemoveAt(i);
            }
                
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
    public static List<GridPos> FindPath(GridPos startPos, GridPos activePos, Type enterObjectType, int maxLength = -1)
    {
        Plan p = new();
        if (!startPos.Equals(activePos))
        {
            LookForPath(startPos, null, new(activePos), p, enterObjectType, maxLength);
            if (p.path.Count == 0)
                p.path = null;
            else if (MyGrid.GetGridItem(p.path[p.path.Count - 1]).GetType() != enterObjectType)
                p.path.RemoveAt(p.path.Count - 1);
        }
        else
        {
            p.path.Add(startPos);
        }
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
    static Task LookForPath(GridPos _start, Building buildingTile, SearchCoords searchCoords, Plan plan, Type enterObjectType, int maxLength)
    {

        Queue queue = new();
        
        // Move if starting inside a building
        if (buildingTile != null)
            _start = BuildingStep(_start, buildingTile.gameObject, -1);
        queue.Enqueue(new(_start, 0, null));

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
                                if (checkNode.minCost == maxLength)
                                    continue;
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
        return t == null || clickable.GetType() == t;
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
        for (int i = 0; i < searchCoords.entryPoints.Count; i++)
        {
            GridPos pos = searchCoords.entryPoints[i];
            if (!checkNode.pos.Equals(pos))
                continue;

            List<GridPos> path = new();
            while (checkNode != null)
            {
                path.Add(checkNode.pos);
                checkNode = checkNode.previous;
            }
            path.Reverse();
            plan.path = path;
            plan.index = i;

            return false;
        }

        if (firstPass)
            return MoveToNewLevel(searchCoords, checkNode, plan, queue);
        return true;
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
                    PathNode outElevatorNode = new(
                        elevatorPos, 
                        inElevatorNode.minCost, 
                        inElevatorNode);

                    ClickableObject el = MyGrid.GetGridItem(elevatorPos);

                    foreach (GridPos pos in (el as Building).entryPoints.EnabledTiles)
                    {
                        PathNode finishMove = new(
                            pos, 
                            outElevatorNode.minCost + 1, 
                            outElevatorNode);
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
        float rotation = building.transform.eulerAngles.y;
        int rot = Mathf.RoundToInt(rotation / 90f);
        return _vec.Switch(rot, mod);
    }

}