

using BuildingStats;
using System;
using System.Collections.Generic;
using System.Text;

namespace Levels
{
    [Serializable]
    public struct BuildingModifications
    {
        public string name;
        public int size;

        public MoneyResource resource;
        public List<StatValue> statValues;
    }
}
