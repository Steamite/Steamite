using System;
using System.Collections.Generic;
using System.Text;
using Unity.Entities;

namespace Assets.Scripts.Systems.Buildings
{
    [Flags]
    public enum BuildingState : byte
    {
        None = 0,
        Constructing = 1,
        Deconstructing = 2,
    }
    public struct BuildingComponent : IComponentData
    {
        public int listIndex;
        public BuildingState state;
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
