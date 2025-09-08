using System;
using UnityEngine;

/// <summary>
/// Class used for unitialized coordnites.<br/>
/// Used for saving insed of Vector3(it has a circular reference that is bad).
/// </summary>
[Serializable]
public class GridPos
{


    /// <summary>X coord on grid.</summary>
    [SerializeField] public float x;
    /// <summary>Y coord on grid.</summary>
    [SerializeField] public float z;
    /// <summary>Level on grid.</summary>
    [SerializeField] public int y;

    #region Base
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
        return $"({x}, {y}, {z})";
    }

    public GridPos Switch(int dirCase, int mod = 1)
    {
        switch (dirCase)
        {
            case 0:
                return new(x, y, z + 1 * mod);
            case 1:
                return new(x + 1 * mod, y, z);
            case 2:
                return new(x, y, z - 1 * mod);
            case 3:
                return new(x - 1 * mod, y, z);
            default:
                throw new ArgumentException();
        }
    }
    #endregion

    #region Constructors
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

    public Vector3 ToVec(float yOffset = 0)
    {
        return new(
            x,
            (y * ClickableObjectFactory.LEVEL_HEIGHT) + yOffset,
            z);
    }
    public Vector3 ToVec(float xOffset, float Yoffset, float zOffset)
    {
        return new(
            x + xOffset,
            (y * ClickableObjectFactory.LEVEL_HEIGHT) + Yoffset,
            z + zOffset);
    }
    public Vector2 ToVecUI()
    {
        if (y != 0)
            Debug.LogError("Probably wrong assigment");
        return new(x, z);
    }
    #endregion
}