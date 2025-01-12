using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

class SearchCoordinates
{
    public List<GridPos> positions;
    public List<GridPos> elevEnterPositions;
    public List<GridPos> elevPositions;
    public List<List<int>> connections;

    public SearchCoordinates(GridPos gridPos)
    {
        positions = new() { gridPos};
        elevEnterPositions = new();
        elevPositions = new();
        connections = new();
    }
    public SearchCoordinates()
    {
        positions = new();
        elevEnterPositions = new();
        elevPositions = new();
        connections = new();
    }
}

public static class PathFinder
{
    const float ROAD_COST = 1;
    const float FASTROUTE_COST = 0.75f;
    const float ELEVATOR_COST = 2f;
    #region Humans
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
                else if(MyGrid.GetGridItem(_start).id != b.id && plan.foundNormaly)
                    plan.path.Add(LastStep(plan.path.Count > 0 ? plan.path[^1] : _start, b.gameObject, 1));
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
    async static Task<Plan> Prep(GridPos _start, List<ClickableObject> objects)
    {
        List<int> entryPoints = new();
        SearchCoordinates coordinates = new();
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
                
                foreach (RectTransform t in MyGrid.GetOverlay(gp.y).buildingOverlays.First(q=> q.name == building.id.ToString()).GetComponentsInChildren<Image>().Select(q=>q.transform))//item in building.blueprint.itemList.Where(q=> q.itemType == GridItemType.Entrance)/*.Skip(1)*/)
                {
                    coordinates.positions.Add(new(Mathf.Floor(t.position.x), gp.y, Mathf.Floor(t.position.z)));
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
                coordinates.positions.Add(objects[i].GetPos());
                entryPoints.Add(i);
            }
        }

        // prep for elevators
        foreach (Building el in MyGrid.buildings.Where(q => q is Elevator))
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
        if(plan.index > -1)
        {
            if (!part)
            {
                plan.path.RemoveAt(0);
            }
            plan.index = entryPoints[plan.index];
        }
        return plan;
    }
    
    public static List<GridPos> FindWayHome(Human h)
    {
        JobData _jData;
        if (h.home)
        {
            _jData = FindPath(new() { h.home }, h);
            if (_jData.interest)
            {
                h.ModifyEfficiency(ModType.House, true);
                return _jData.path;
            }
        }
        Building elevator = MyGrid.buildings.First(q => q is Elevator el && el.main);
        _jData = FindPath(new() { elevator }, h);
        h.ModifyEfficiency(ModType.House, false);
        if (_jData.interest)
        {
            return _jData.path;
        }
        return new();
    }
#endregion Humans

    public static List<GridPos> FindPath(GridPos startPos, GridPos activePos, Type enterObjectType)
    {
        Plan p = new();
        if (!startPos.Equals(activePos))
            LookForPath(startPos, null, new(activePos), p, enterObjectType);
        return p.path;
    }
    
    #region Looping
    static Task LookForPath(GridPos _start, Building buildingTile, SearchCoordinates searchCoords, Plan plan, Type enterObjectType)
    {
        // Move if starting inside a building
        if (buildingTile != null)
        {
            _start = LastStep(_start, buildingTile.gameObject, -1);
        }

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
                        if(Check(checkNode, searchCoords, plan, queue))
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
    static bool CanEnter(ClickableObject clickable, Type t)
    {
        return clickable.GetType() == t;
        /*if(t == null)
        {
            return true;
        }
        else
        {
            return MyGrid.GetGridItem(vec, t == typeof(Pipe))?.GetType() == t;
        }*/
    }
    static bool Check(PathNode checkNode, SearchCoordinates searchCoords, Plan plan, Queue queue, bool firstPass = true)
    {
        int id = -1;
        int count = 0;
        foreach (GridPos pos in searchCoords.positions)
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

    static bool MoveToNewLevel(SearchCoordinates searchCoords, PathNode checkNode, Plan plan, Queue queue)
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

    static GridPos LastStep(GridPos _vec, GameObject building, int mod)
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