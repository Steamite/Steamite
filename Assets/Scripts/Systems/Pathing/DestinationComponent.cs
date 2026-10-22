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

namespace Assets.Scripts.Systems.Pathing
{
    public struct DestinationComponent : IComponentData
    {
        public int pos;
        public int2 worldPos;
        [ReadOnly] public NativeArray<LevelGridTile> grid;
        public int width;
        public int offset;
    }

    public struct PathfindingComponent : IComponentData
    {
        public float3 position;
        public int currentWaypoint;
        public float speed;
    }
}
