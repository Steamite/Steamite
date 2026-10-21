using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Unity.Entities;
using Unity.Mathematics;
using UnityEditor.Rendering;
using UnityEngine;

namespace Assets.Scripts.Systems
{
    public class HumanAuthoring : MonoBehaviour
    {
        [SerializeField] float speed;
        [SerializeField] Vector3 pos;
        [SerializeField] List<Vector3> points;

        class HumanBaker : Baker<HumanAuthoring>
        {
            
            public override void Bake(HumanAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                    
                AddComponent(entity, new HumanComponent
                {
                    position = authoring.pos,
                    currentWaypoint = 0,
                    speed = authoring.speed
                });
                var buffer = AddBuffer<PositionNode>(entity);
                for (int i = 0; i < authoring.points.Count; i++)
                {
                    buffer.Add(new() { position = authoring.points[i] });
                }

                var prefab = GetEntity(TransformUsageFlags.None);
                AddComponent(prefab, new HumanPrefab() { entity = entity });
            }
        }
    }

    public struct HumanPrefab : IComponentData
    {
        public Entity entity;
    }

    public struct HumanComponent : IComponentData
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
}
