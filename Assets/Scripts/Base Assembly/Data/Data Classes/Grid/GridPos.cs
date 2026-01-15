using System;
using UnityEngine;

/// <summary>
/// Class used for unitialized coordnites.<br/>
/// Used for saving insed of Vector3(it has a circular reference that is bad).
/// </summary>
[Serializable]
public struct GridPos
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
        y = 0;
    }
    public GridPos(Vector3 vec)
    {
        x = vec.x;
        y = Mathf.RoundToInt(vec.y);
        z = vec.z;
    }

    public static GridPos operator +(GridPos a, GridPos b)
        => new GridPos(a.x + b.x, a.z + b.z);

    public static GridPos operator -(GridPos a, GridPos b)
        => new GridPos(a.x - b.x, a.z - b.z);
    #endregion

    #region Functions



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

    /// <summary>
    /// Rotates the <paramref name="offset"/> by rotation.
    /// </summary>
    /// <param name="offset">Original value.</param>
    /// <param name="rotation">Dermining value</param>
    /// <param name="isTile">If it's a building or a tile.</param>
    /// <returns>Rotated <paramref name="offset"/>.</returns>
    public GridPos Rotate(float rotation, bool isTile = false)
    {
        GridPos gp;
        switch (rotation)
        {
            case 90:
                if (isTile)
                    gp = new(-z, x);
                else
                    gp = new(z, -x);
                break;
            case 180:
                gp = new(-x, -z);
                break;
            case 270:
                if (isTile)
                    gp = new(z, -x);
                else
                    gp = new(-z, x);
                break;
            default:
                gp = new(x, z);
                break;
        }
        return gp;
    }


    #endregion
}