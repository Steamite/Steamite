using System;
using System.Collections.Generic;
using System.Text;
using Unity.Entities;

namespace Assets.Scripts.Systems.Buildings
{
    public struct BuildingComponent : IComponentData
    {
        public int typeIndex;

    }

    [InternalBufferCapacity(3)]
    public struct BuildingInventory : IBufferElementData
    {
        /// <summary>
        /// 0 - 3
        /// 4 - 7
        /// </summary>
        public byte type;
        public byte stored;
        public byte cost;
    }

    [InternalBufferCapacity(3)]
    public struct ProductionData : IBufferElementData
    {
        /// <summary>
        /// 0 - 3
        /// 4 - 7
        /// </summary>
        public byte type;
        public byte stored;
        public byte reward;
    }
}
