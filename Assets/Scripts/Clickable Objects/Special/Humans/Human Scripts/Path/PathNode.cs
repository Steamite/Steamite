using System;

/// <summary>One tile/node in path, used in pathfinding.</summary>
class PathNode
{
    /// <summary>node position</summary>
    public GridPos pos;
    /// <summary>cost to reach</summary>
    public float minCost;
    /// <summary>previus node, forms a chain back to start</summary>
    public PathNode previous;
    public PathNode(GridPos _pos, float _minCost, PathNode _previous)
    {
        pos = _pos;
        minCost = _minCost;
        previous = _previous;
    }
    /// <summary>
    /// Used in the search swich.
    /// </summary>
    /// <param name="_i">for cycle index</param>
    /// <param name="_previous">previus node</param>
    /// <exception cref="ArgumentException">If <paramref name="_i"/> is out of range.</exception>
    public PathNode(int _i, PathNode _previous)
    {
        switch (_i)
        {
            case 0:
                pos = new(_previous.pos.x + 1, _previous.pos.y, _previous.pos.z);
                break;
            case 1:
                pos = new(_previous.pos.x - 1, _previous.pos.y, _previous.pos.z);
                break;
            case 2:
                pos = new(_previous.pos.x, _previous.pos.y, _previous.pos.z + 1);
                break;
            case 3:
                pos = new(_previous.pos.x, _previous.pos.y, _previous.pos.z - 1);
                break;
            default:
                throw new ArgumentException();
        }
        previous = _previous;
        minCost = _previous.minCost;
    }
    public override bool Equals(object obj)
    {
        if (obj != null)
        {
            if (obj is PathNode)
            {
                return ((PathNode)obj).pos.Equals(pos);
            }
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(pos, minCost, previous);
    }
}