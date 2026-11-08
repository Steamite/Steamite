using Unity.Collections;
using Unity.Entities;

namespace Assets.Scripts.Systems.Human.JobFind
{
    public struct JobOfferComponent : IComponentData
    {
        /// <summary>
        /// Positions of the rocks.
        /// </summary>
        public NativeList<int> digJobs;
        public NativeList<int> impossibleToDig;
        /// <summary>
        /// Building to work on
        /// </summary>
        public NativeList<Entity> buildingJobs;
    }


    /*public struct DigJob
    {
        int rockPosition;
    }*/
}
