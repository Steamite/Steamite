using System;
using UnityEngine;

[Serializable]
public class GridPos
{
    [SerializeField]
    public float x;
    [SerializeField]
    public float z;
    [SerializeField]
    public float level;

    // override object.Equals
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        GridPos pos = (GridPos)obj;
        if (pos.x == x && pos.z == z && pos.level == level)
        {
            return true;
        }
        return false;
    }
    public override int GetHashCode() { return base.GetHashCode(); }
    public GridPos()
    {

    }
    public GridPos(float _x, float _level, float _z)
    {
        x = _x;
        level = _level;
        z = _z;
    }
    public GridPos(float _x, float _z)
    {
        x = _x;
        z = _z;
    }
    public GridPos(Vector3 vec, bool round = true)
    {
        level = 0;
        if (round)
        {
            x = Mathf.RoundToInt(vec.x);
            z = Mathf.RoundToInt(vec.z);
        }
        else
        {
            x = Mathf.FloorToInt(vec.x);
            z = Mathf.FloorToInt(vec.z);
        }
    }
    public GridPos(GameObject g)
    {
        level = 0;
        x = Mathf.RoundToInt(g.transform.position.x);
        z = Mathf.RoundToInt(g.transform.position.z);
    }
    public Vector3 ToVec()
    {
        return new(x, level * 2, z);
    }
}