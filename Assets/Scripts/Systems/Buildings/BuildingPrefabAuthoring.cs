using System;
using System.Runtime.CompilerServices;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.Systems.Buildings
{
    public class BuildingPrefabAuthoring : MonoBehaviour
    {
        [SerializeField] BuildingData buildingData;
        public class BuildingPrefabBaker : Baker<BuildingPrefabAuthoring>
        {
            public override void Bake(BuildingPrefabAuthoring authoring)
            {
                var ent = GetEntity(TransformUsageFlags.None);
                var a = SetBuffer<BuildingPrefabsComponent>(ent);
                for (int i = 0; i < authoring.buildingData.Categories.Count; i++)
                {
                    /*var en =
                    a.Add(en);*/
                }
            }
        }
    }

    [InternalBufferCapacity(30)]
    public struct BuildingPrefabsComponent : IBufferElementData
    {
        public Entity prefab;
    }

    public struct BuildingComponent : IComponentData
    {
        public int typeIndex;

    }
}
