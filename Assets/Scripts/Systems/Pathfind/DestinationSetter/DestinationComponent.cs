using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using BuildingStats;
using NUnit;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.LightTransport;

namespace Pathing
{
    public struct DestinationComponent : IComponentData
    {
        public NativeArray<int> positions;
        public int level;
    }

    public struct WorldComponent : IComponentData
    {
        [ReadOnly] public NativeArray<short> path;
        [ReadOnly] public NativeArray<int> offsets;
        [ReadOnly] public int LargestLevelDimensionScaled;
        [ReadOnly] public int largestGridSize;
    }
}
