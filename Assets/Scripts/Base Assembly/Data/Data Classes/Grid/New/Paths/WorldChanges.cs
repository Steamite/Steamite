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
        public static void SetTile(int localIndex, int levelIndex, in LevelGridTile tile, ref WorldData worldData)
        {
            // Update path
            PathChanges.UpdatePath(localIndex, levelIndex, tile, ref worldData);

            // affected level
            var level = worldData.levels[levelIndex];

            if (tile.Type != level.renderGrid[localIndex].Type)
                DirtyGridUtils.SetDirty(
                    level.dirtyChunks,
                    level.dimScaled,
                    level.dim,
                    localIndex,
                    tile.TType == TileType.Wall ? 2 : 1);

            // assign
            level.renderGrid[localIndex] = tile;
        }
    }
}
