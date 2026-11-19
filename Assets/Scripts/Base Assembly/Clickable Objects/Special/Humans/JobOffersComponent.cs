using Unity.Collections;
using Unity.Entities;

namespace Assets.Scripts.Systems.Human.JobFind
{
    public struct JobOffersComponent : IComponentData
    {
        /// <summary>
        /// Absolute world positions of the rocks.
        /// </summary>
        public NativeList<int> digJobs;
        /// <summary>
        /// Stores regions of reachable rocks, for faster pathfinding.
        /// </summary>
        public NativeList<uint> digRegions;

        public NativeList<int> impossibleToDig;

        public NativeList<DigAssign> assignedJobs;


        /// <summary>
        /// Building to work on
        /// </summary>
        public NativeList<Entity> buildingJobs;
    }

    public struct DigAssign
    {
        public int position;
        public Entity assignHuman;
    }


    public struct JobCancelComponent : IComponentData
    {
        /// <summary>
        /// Canceled by player
        /// </summary>
        public NativeList<int> positionsToCancel;
    }
}
