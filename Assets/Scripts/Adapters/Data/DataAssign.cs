using System;

[Serializable]
public struct DataAssign
{
    public int categoryId;
    public int objectId;

    public DataAssign(int _categoryIndex, int _objectId)
    {
        categoryId = _categoryIndex;
        objectId = _objectId;
    }
}

