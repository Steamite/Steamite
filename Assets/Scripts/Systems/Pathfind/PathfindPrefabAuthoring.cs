using Humans;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Pathing
{
    public class PathfindPrefabAuthoring : MonoBehaviour
    {
        [SerializeField] HumanPrefabData prefabData;

        class PathfindPrefabBaker : Baker<PathfindPrefabAuthoring>
        {
            public override void Bake(PathfindPrefabAuthoring authoring)
            {
                int count = authoring.prefabData.count;
                float speed = authoring.prefabData.speed;
                int3 pos = authoring.prefabData.position;

                var ent = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<Prefab>(ent);

                AddComponent<PathfindComponent>(ent, new() { position = 6008});
                AddComponent<HumanJobData>(ent, new() { jobType = JobType.Free, buildingInterest = default, targetPosition = -1 });
                AddComponent<HumanIsFree>(ent, new());
                AddBuffer<PositionNode>(ent);

                var e = CreateAdditionalEntity(TransformUsageFlags.None);
                AddComponent<HumanPrefab>(e, new() { entity = ent, count = count, speed = speed, pos = pos});
            }
        }
    }

    public struct HumanPrefab : IComponentData
    {
        public float speed;
        public int count;
        public Entity entity;
        internal int3 pos;
    }
}
