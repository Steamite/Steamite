using System.Collections.Generic;
using System;

/// <summary>Pathfinding result, needs to parsed to <see cref="JobData"/>.</summary>
[Serializable]
public class Plan
{
    public List<GridPos> path = new();
    public int index = -1; // index in objects
    public bool foundNormaly = true;

    public Plan(List<GridPos> _path, int _index)
    {
        path = _path;
        index = _index;
    }
    public Plan()
    {

    }
}
