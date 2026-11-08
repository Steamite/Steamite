using System;
using System.Collections.Generic;
using System.Text;
using Unity.Entities;
using Unity.Mathematics;

namespace Humans
{
    [InternalBufferCapacity(3)]
    public struct HumanInventory : IBufferElementData
    {
        /// <summary>
        /// 0 - 3
        /// 4 - 7
        /// </summary>
        public byte type;
        public byte stored;
        public byte ordered;
    }

    public struct HumanReferences : IComponentData
    {
        public Entity interest;
        public Entity destination;
        public Entity house;
    }

    public struct PathfindComponent : IComponentData
    {
        public float3 position;
        public int currentWaypoint;
        public float speed;
    }


    [InternalBufferCapacity(20)]
    public struct PositionNode : IBufferElementData
    {
        public float3 position;
    }

    public struct HumanJobData : IComponentData
    {
        public int targetLevel;
        public int targetPosition;
        public Entity buildingInterest;

        public JobType jobType;
    }
    public enum JobType
    {
        Free,
        Dig,
        Construct
    }
    /*
        [InternalBufferCapacity(3)]
        public struct OrderedResources : IBufferElementData
        {
            public byte type;
            public byte ammount;
        }

        [InternalBufferCapacity(3)]
        public struct RequestedResources : IBufferElementData
        {
            public byte type;
            public byte ammount;
        }

        [InternalBufferCapacity(3)]
        public struct OrderedResource : IBufferElementData
        {
            // 2
            /// <summary>
            /// 0 - 3
            /// 4 - 7
            /// </summary>
            public byte type;
            public byte change;
            public bool take;

            // 8
            public Entity Human;
        }*/

}
