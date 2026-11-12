using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using Pathing;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.Systems.Pathfind.DestinationSetter
{
    public class WorldLinker : MonoBehaviour, IWorldInit
    {
        NativeArray<int> offsets;
        public void Init(GridWorld gridWorld)
        {
            var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var ent = manager.CreateEntity();

            offsets = gridWorld.offsetPathTableNeighbour;


            manager.AddComponentData<WorldComponent>(ent, new()
            {
                path = gridWorld.path,
                offsets = offsets,
                LargestLevelDimensionScaled = gridWorld.LargestLevelDimensionScaled,
                largestGridSize = gridWorld.GridSizeScaled
            });

            var buffer = manager.AddBuffer<LevelComponent>(ent);
            for (int i = 0; i < gridWorld.LEVEL_COUNT; i++)
            {
                var level = gridWorld.gridLevels[i];

                
                buffer.Add(new()
                {
                    renderGrid = level.renderGrid,
                    neighbours = level.neighbourOffsets,
                    dimensionsScaled = level.dimensionsScaled,
                    dimensions = level.levelDimensions,
                    chunkCount = level.levelDimensions * level.levelDimensions,
                    offsetsScaled = level.offsetScaled,
                    offsets = level.levelOffset,
                    dirtyChunks = level.dirtyChunks
                });
            }
        }
    }
}
