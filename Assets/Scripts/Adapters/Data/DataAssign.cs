using System;

[Serializable]
public struct DataAssign : IEquatable<DataAssign>
{
    public int categoryId;
    public int objectId;

    public DataAssign(int _categoryIndex, int _objectId)
    {
        categoryId = _categoryIndex;
        objectId = _objectId;
    }

    public override bool Equals(object obj)
    {
        return obj is DataAssign assign && Equals(assign);
    }

    public bool Equals(DataAssign other)
    {
        return categoryId == other.categoryId &&
               objectId == other.objectId;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(categoryId, objectId);
    }

    public static bool operator ==(DataAssign left, DataAssign right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(DataAssign left, DataAssign right)
    {
        return !(left == right);
    }
}

