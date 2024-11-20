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
    public int y;

    // override object.Equals
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        GridPos pos = (GridPos)obj;
        if (pos.x == x && pos.z == z && pos.y == y)
        {
            return true;
        }
        return false;
    }
    public override int GetHashCode() { return base.GetHashCode(); }
    public override string ToString()
    {
        return $"x: {x}, y: {y}, z: {z}";
    }
    public GridPos()
    {

    }
    public GridPos(float _x, float _level, float _z)
    {
        x = _x;
        y = Mathf.RoundToInt(_level);
        z = _z;
    }
    public GridPos(float _x, float _z)
    {
        x = _x;
        z = _z;
    }
    public GridPos(Vector3 vec)
    {
        x = vec.x;
        y = Mathf.RoundToInt(vec.y);
        z = vec.z;
    }
    public GridPos(GameObject g)
    {
        Vector3 vec = g.transform.localPosition;
        x = vec.x;
        y = Mathf.RoundToInt(vec.y / 2);
        z = vec.z;
    }

    public Vector3 ToVec(float Yoffset = 0)
    {
        return new(
            x, 
            (y * 2) + Yoffset,
            z);
    }
}