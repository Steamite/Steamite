using System.Collections.Generic;
using System;

[Serializable]
public class Plan
{
    public List<GridPos> path = new();
    public int index = -1; // index in objects

    public Plan(List<GridPos> _path, int _index)
    {
        path = _path;
        index = _index;
    }
    public Plan()
    {

    }
}
