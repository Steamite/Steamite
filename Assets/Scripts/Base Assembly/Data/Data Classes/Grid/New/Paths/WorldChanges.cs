using Rendering;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Burst;
using Unity.Collections;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.Paths
{
    [BurstCompile]

    public static class WorldChanges
    {
        [BurstCompile]
        public static void SetTile(int localIndex, int levelIndex, in LevelGridTile tile, ref WorldComponent worldData, in NativeArray<LevelComponent> levels)
        {
            // Update path
            PathChanges.UpdatePath(localIndex, levelIndex, tile, ref worldData, in levels);

            // affected level
            var level = levels[levelIndex];

            if (tile.Type != level.renderGrid[localIndex].Type)
                DirtyGridUtils.SetDirty(
                    level.dirtyChunks,
                    level.dimensionsScaled,
                    level.dimensions,
                    localIndex,
                    tile.TType == TileType.Wall ? 2 : 1);

            // assign
            level.renderGrid[localIndex] = tile;
        }
    }
}
