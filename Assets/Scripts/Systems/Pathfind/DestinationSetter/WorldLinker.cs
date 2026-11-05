using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using Assets.Scripts.Systems.Pathing;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.Systems.Pathfind.DestinationSetter
{
    public class WorldLinker : MonoBehaviour, IAfterLoad
    {
        NativeArray<int> offsets;
        public void AfterLoad()
        {
            var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var ent = manager.CreateEntity();

            GridWorld world = SceneRefs.World;


            offsets = new NativeArray<int>(4, Allocator.Persistent);
            offsets[0] = -1;
            offsets[1] = -world.LargestLevelDimensionScaled;
            offsets[2] = 1;
            offsets[3] = world.LargestLevelDimensionScaled;


            manager.AddComponentData<WorldComponent>(ent, new()
            {
                path = world.path,
                offsets = offsets,
                LargestLevelDimensionScaled = world.LargestLevelDimensionScaled,
                largestGridSize = world.GridSizeScaled
            });
        }

        private void OnDestroy()
        {
            offsets.Dispose();
        }
    }
}
