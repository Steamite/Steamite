using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Pathing
{
    public struct DestinationComponent : IComponentData
    {
        public NativeArray<int> positions;
        public int level;
    }
}
