using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public struct DataAssign
{
    public int categoryIndex;
    public int objectId;

    public DataAssign(int _categoryIndex, int _objectId)
    {
        categoryIndex = _categoryIndex;
        objectId = _objectId;
    }
}

