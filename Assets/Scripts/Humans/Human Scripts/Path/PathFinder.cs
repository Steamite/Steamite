using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

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
                Debug.Log("");
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
                foreach (RectTransform t in MyGrid.sceneReferences.overlay.buildingOverlays.First(q=> q.name == building.id.ToString()).GetComponentsInChildren<Image>().Select(q=>q.transform))//item in building.build.blueprint.itemList.Where(q=> q.itemType == GridItemType.Entrance)/*.Skip(1)*/)
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
    static void LookForPath(GridPos _start, Building buildingTile, List<GridPos> positions, Plan plan, Type enterObjectType)
    {
        int check = 0;
        List<GridPos> visited = new();
        List<List<GridPos>> paths = new();
        List<int> toBeRemoved = new();
        bool fin = false;
        paths.Add(new());
        paths[0].Add(_start);
        int i;
        
        if (buildingTile != null)
        {
            i = MyGrid.buildings.Select(q => q.id).ToList().IndexOf(buildingTile.id);
            if (i > -1)
            {
                GridPos vec = LastStep(_start, MyGrid.buildings[i].gameObject, -1);
                paths[0].Add(vec);
            }
        }
        i = 0;

        if (!fin && Check(paths[0][^1], 0,paths, positions, check, plan)) // Am I standing on an entry point or next to the job
        {
            //Am I startring on a building
            while (i < 30 && paths.Count > 0 && !fin) // when finished, when no paths, when out of range
            {
                toBeRemoved = new();
                int c = paths.Count;
                for (int j = 0; j < c; j++) // foreach active path
                {
                    if(plan.index == -1)
                    {
                        CheckMove(j, visited, paths, positions, check, toBeRemoved, plan, enterObjectType);
                    }
                    else
                    {
                        return;
                    }
                }
                if (paths.Count > 0) // removes no longer active paths
                {
                    for (int x = toBeRemoved.Count - 1; x >= 0; x--)
                    {
                        int y = toBeRemoved[x];
                        paths.RemoveAt(y);
                    }
                }
                i++;
            }
        }
    }
    static void CheckMove(int pathIndex, List<GridPos> visited, List<List<GridPos>> paths, List<GridPos> positions, int check, List<int> toBeRemoved, Plan plan, Type enterObjectType)
    {
        try
        {
            // from where
            GridPos vec = paths[pathIndex][^1];
            check = 0;
            for (int i = 0; i < 4; i++) // checks in every direction
            {
                GridPos checkVec = new();
                switch (i)
                {
                    case 0:
                        checkVec = new(vec.x + 1, vec.z); 
                        break;
                    case 1:
                        checkVec = new(vec.x - 1, vec.z);
                        break;
                    case 2:
                        checkVec = new(vec.x, vec.z + 1);
                        break;
                    case 3:
                        checkVec = new(vec.x, vec.z - 1);
                        break;
                }
                if (visited.Where(q=>q.Equals(checkVec)).Count() == 0) // checks if already visited
                {
                    visited.Add(checkVec);
                    if (Check(checkVec, pathIndex, paths, positions, check, plan)) // if nothing found
                    {
                        if (CanEnter(checkVec, enterObjectType)) // if there is a road on the new position
                        {
                            if (check == 0) // if first then add
                            {
                                paths[pathIndex].Add(checkVec);
                                check++;
                            }
                            else // else create a new branch
                            {
                                paths.Add(paths[pathIndex].ToList());
                                paths[^1][^1] = checkVec;
                                check++;
                            }
                        }
                    }
                    else
                    {
                        return;
                    }
                }
            }
            if (check == 0)
            {
                toBeRemoved.Add(pathIndex); // if none found from this position delete the whole path
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("haaha it's null you dumbass: " +paths.Count+ "\n"+ e); // log error
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
    static bool Check(GridPos checkVec, int pathIndex, List<List<GridPos>> paths, List<GridPos> positions, int check, Plan plan)
    {
        int id = -1;
        int count = 0;
        foreach (GridPos pos in positions)
        {
            if (checkVec.Equals(pos))
            {
                id = count;
                break;
            }
            count++;
        }
        if (id > -1) // if there is an entry point or a job on the checkVec
        {
            if (check != 0) // removes the last element of the path if there already is one
            {
                paths[pathIndex].RemoveAt(paths[pathIndex].Count-1);
            }
            paths[pathIndex].Add(checkVec); // add the new pos to the path
            paths[pathIndex].RemoveAt(0);
            plan.path.AddRange(paths[pathIndex].ToList());
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