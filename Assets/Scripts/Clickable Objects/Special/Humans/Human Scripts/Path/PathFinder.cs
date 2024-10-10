using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

class PathNode
{
    public GridPos pos;
    public PathNode previus;
    public PathNode(GridPos gp, PathNode p)
    {
        pos = gp;
        previus = p;
    }
}

public static class PathFinder
{
    public static JobData FindPath(List<ClickableObject> objects, Human h)
    {
        if (objects.Count == 0)
            return new();
        GridPos _start = new(h.transform.position);
        Plan plan = Prep(_start, objects);
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
                else if(MyGrid.GetGridItem(_start).id != b.id)
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
            h.OpenWindow();
            return new(_path: plan.path, _interest: interest);
        }
        return new JobData(_path: new(), _interest: null);
    }
    static Plan Prep(GridPos _start, List<ClickableObject> objects)
    {
        List<GridPos> positions = new();
        List<int> entryPoints = new();
        Building part = MyGrid.GetGridItem(_start) as Building; // gets tile build reference if standing on it
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
                Pipe pipe = building.GetComponent<Pipe>();
                if (pipe)
                {
                    positions.Add(new(pipe.transform.position));
                    entryPoints.Add(i);
                    continue;
                }
                foreach (RectTransform t in MyGrid.canvasManager.overlays.buildingOverlays.First(q=> q.name == building.id.ToString()).GetComponentsInChildren<Image>().Select(q=>q.transform))//item in building.build.blueprint.itemList.Where(q=> q.itemType == GridItemType.Entrance)/*.Skip(1)*/)
                {
                    positions.Add(new(t.position));
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
                positions.Add(new(objects[i].transform.position));
                entryPoints.Add(i);
            }
        }
        LookForPath(_start, part, positions, plan, typeof(Road));
        if(plan.index > 0)
            plan.index = entryPoints[plan.index];
        return plan;
    }
    public static List<GridPos> FindPath(GridPos startPos, GridPos activePos, Type enterObjectType)
    {
        Plan p = new();
        if (!startPos.Equals(activePos))
            LookForPath(startPos, null, new() { activePos }, p, enterObjectType);
        return p.path;
    }
    public static List<GridPos> FindWayHome(Human h)
    {
        JobData _jData;
        if (h.home)
        {
            _jData = FindPath(new() { h.home }, h);
            if (_jData.interest)
            {
                h.efficiency.ManageModifier(ModType.House, true);
                return _jData.path;
            }
        }
        Building elevator = MyGrid.buildings.First(q => (Elevator)q != null && ((Elevator)q).main);
        _jData = FindPath(new() { elevator }, h);
        h.efficiency.ManageModifier(ModType.House, false);
        if (_jData.interest)
        {
            return _jData.path;
        }
        return new();
    }

    static void LookForPath(GridPos _start, Building buildingTile, List<GridPos> positions, Plan plan, Type enterObjectType)
    {
        List<GridPos> visited = new();
        List<PathNode> toCheck = new();
        bool fin = false;
        toCheck.Add(new(_start, null)); 
        visited.Add(_start);
        int i;
        
        if (buildingTile != null)
        {
            i = MyGrid.buildings.Select(q => q.id).ToList().IndexOf(buildingTile.id);
            if (i > -1)
            {
                GridPos vec = LastStep(_start, MyGrid.buildings[i].gameObject, -1);
                toCheck[0] = new(vec, toCheck[0]);
            }
        }
        i = 0;

        if (!fin && Check(toCheck[0], positions, plan)) // Am I standing on an entry point or next to the job
        {
            //Am I startring on a building
            while (i < 30 && toCheck.Count > 0 && !fin) // when finished, when no paths, when out of range
            {
                int c = toCheck.Count;
                for (int j = 0; j < c; j++) // foreach active path
                {
                    if(plan.index == -1)
                    {
                        CheckMove(visited, toCheck, j, positions, plan, enterObjectType);
                    }
                    else
                    {
                        return;
                    }
                }
                toCheck.RemoveRange(0, c);
                i++;
            }
        }
    }
    static void CheckMove(List<GridPos> visited, List<PathNode> pathNodes, int j, List<GridPos> positions, Plan plan, Type enterObjectType)
    {
        try
        {
            // from where
            PathNode pathNode = pathNodes[j];
            GridPos vec = pathNode.pos;
            for (int i = 0; i < 4; i++) // checks in every direction
            {
                PathNode checkNode;
                switch (i)
                {
                    case 0:
                        checkNode = new(new(vec.x + 1, vec.z), pathNode); 
                        break;
                    case 1:
                        checkNode = new(new(vec.x - 1, vec.z), pathNode);
                        break;
                    case 2:
                        checkNode = new(new(vec.x, vec.z + 1), pathNode);
                        break;
                    case 3:
                        checkNode = new(new(vec.x, vec.z - 1), pathNode);
                        break;
                    default:
                        continue;
                }
                if (visited.Where(q=>q.Equals(checkNode.pos)).Count() == 0) // checks if already visited
                {
                    visited.Add(checkNode.pos);
                    if (Check(checkNode, positions, plan)) // if nothing found
                    {
                        if (CanEnter(checkNode.pos, enterObjectType)) // if there is a road on the new position
                        {
                            pathNodes.Add(checkNode);
                        }
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("haaha it's null you dumbass: " + pathNodes.Count+ "\n"+ e); // log error
        }

        return;
    }
    static bool CanEnter(GridPos vec, Type t)
    {
        if(t == null)
        {
            return true;
        }
        else if(t == typeof(Pipe))
        {
            return MyGrid.pipeGrid[(int)vec.x, (int)vec.z]?.GetType() == t;
        }
        else
        {
            return MyGrid.GetGridItem(vec).GetType() == t;
        }
    }
    static bool Check(PathNode checkNode, List<GridPos> positions, Plan plan)
    {
        int id = -1;
        int count = 0;
        foreach (GridPos pos in positions)
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
            while(checkNode != null)
            {
                path.Add(checkNode.pos);
                checkNode = checkNode.previus;
            }
            path.Reverse();
            plan.path = path;
            plan.index = id;
            return false;
        }
        else
        {
            return true;
        }
    }
    static GridPos LastStep(GridPos _vec, GameObject build, int mod)
    {
        GridPos vec = new(_vec.x, _vec.z);
        float rotation = build.transform.eulerAngles.y;
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
        return new(vec.x, vec.z);
    }

    
}