using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.Humans;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts.Systems.Pathing
{
    public class PathfindPrefabAuthoring : MonoBehaviour
    {
        [SerializeField] int count;
        [SerializeField] float speed;

        class PathfinderPrefabBaker : Baker<PathfindPrefabAuthoring>
        {
            public override void Bake(PathfindPrefabAuthoring authoring)
            {
                var ent = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<Prefab>(ent);

                AddComponent<PathfindingComponent>(ent, new() { position = 6008});
                AddBuffer<PositionNode>(ent);
                var e = CreateAdditionalEntity(TransformUsageFlags.None);
                AddComponent<PathfindPrefab>(e, new() { entity = ent, count = authoring.count, speed = authoring.speed});
            }
        }
    }

    public struct PathfindPrefab : IComponentData
    {
        public float speed;
        public int count;
        public Entity entity;
    }
}
