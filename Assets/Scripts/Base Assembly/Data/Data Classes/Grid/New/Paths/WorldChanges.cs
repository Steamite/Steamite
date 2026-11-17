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
        /// <summary>
        /// Only updates the path and tile info.
        /// Used in creating the new level.
        /// </summary>
        /// <param name="localIndex"></param>
        /// <param name="levelIndex"></param>
        /// <param name="tile"></param>
        /// <param name="worldData"></param>
        /// <param name="levels"></param>
        [BurstCompile]
        public static void SetTileCreate(
            int localIndex, 
            int levelIndex, 
            in LevelGridTile tile, 
            ref WorldComponent worldData, 
            in NativeArray<LevelComponent> levels)
        {
            // Affected level
            var level = levels[levelIndex];

            // Update path
            PathChanges.UpdatePath(localIndex, levelIndex, tile, ref worldData, in levels);

            // assign
            level.renderGrid[localIndex] = tile;
        }

        /// <summary>
        /// Use in runtime.
        /// Updates paths, regions and requests a mesh recalculation (if the type changed).
        /// And in the end sets the tile.
        /// </summary>
        /// <param name="localIndex"></param>
        /// <param name="levelIndex"></param>
        /// <param name="tile"></param>
        /// <param name="worldData"></param>
        /// <param name="levels"></param>
        [BurstCompile]
        public static void SetTileRuntime(int localIndex, int levelIndex, in LevelGridTile tile, ref WorldComponent worldData, in NativeArray<LevelComponent> levels)
        {
            var level = levels[levelIndex];

            if (tile.Type != level.renderGrid[localIndex].Type)
            {
                PathChanges.UpdatePath(localIndex, levelIndex, tile, ref worldData, in levels);

                int worldIndex = Recalculation.WorldIndexFromLocalIndex(localIndex, level.offsetsScaled, level.dimensionsScaled);
                RegionChanges.SetPath(
                    worldIndex,
                    worldData,
                    levelIndex);

                DirtyGridUtils.SetDirty(
                    level.dirtyChunks,
                    level.dimensionsScaled,
                    level.dimensions,
                    localIndex,
                    tile.TType == TileType.Wall ? 2 : 1);
            }
                

            level.renderGrid[localIndex] = tile;
        }
    }
}
