using System;
using System.Collections.Generic;
using System.Text;
using Unity.Burst;
using Unity.Collections;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New
{
    
    public static class PathChanges
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="localIndex"></param>
        /// <param name="levelIndex"></param>
        /// <param name="tile"></param>
        /// <param name="dimensionScaled"></param>
        /// <param name="offsetScaled"></param>
        /// <param name="worldOffset">Offset using largest dim scaled</param>
        /// <param name="levelOffset">Offset using level dim scaled.</param>
        /// <param name="renderGrid"></param>
        /// <param name="path"></param>
        
        public static void UpdatePath(
            int localIndex, 
            int levelIndex,
            LevelGridTile tile,
            int dimensionScaled,
            int offsetScaled,
            NativeArray<int> worldOffset,
            NativeArray<int> levelOffset,
            NativeArray<LevelGridTile> renderGrid,
            NativeArray<short> path)
        {
            TileType newTileType = tile.TType;
            TileType prevTileType = renderGrid[localIndex].TType;

            int worldIndex = Recalculation.WorldIndexFromLocalIndex(localIndex, offsetScaled, dimensionScaled);
            
            if (newTileType == TileType.Road || newTileType == TileType.PassableBuilding || newTileType == TileType.Elevator)
            {
                SetPath(
                    localIndex, 
                    newTileType, 
                    prevTileType, 
                    worldIndex, 
                    levelIndex, 
                    worldOffset, 
                    levelOffset, 
                    renderGrid, 
                    path);
            }
            else
            {
                ClearPath(
                    tile,
                    worldIndex,
                    worldOffset,
                    path);
            }
        }

        static void SetPath(
            int localIndex, 
            TileType newTileType, 
            TileType prevTileType, 
            int worldIndex, 
            int levelIndex,
            NativeArray<int> worldOffset,
            NativeArray<int> levelOffset,
            NativeArray<LevelGridTile> renderGrid,
            NativeArray<short> path)
        {
            for (int i = 0; i < 4; i++)
            {
                int pathIndex = worldIndex - worldOffset[i];
                int gridIndex = localIndex + levelOffset[i];

                if (renderGrid[gridIndex].TType == TileType.Building)
                    continue;
                path[pathIndex] = (short)(path[pathIndex] | (short)(0x1 << i));
            }

            // was building
            HandleBuildingPath(localIndex, newTileType, prevTileType, worldIndex, levelIndex, worldOffset, levelOffset, renderGrid, path);
        }

        static void HandleBuildingPath(
            int localIndex, 
            TileType newTileType, 
            TileType prevTileType, 
            int worldIndex,
            int levelIndex,
            NativeArray<int> worldOffset,
            NativeArray<int> neighbourOffset,
            NativeArray<LevelGridTile> renderGrid,
            NativeArray<short> path)
        {
            if (prevTileType == TileType.Building)
            {
                short newPath = 0;
                for (int i = 0; i < 4; i++)
                {
                    int gridIndex = localIndex - worldOffset[i];
                    bool canEnter =
                        renderGrid[gridIndex].TType == TileType.Road
                        ||
                        (renderGrid[gridIndex].TType > TileType.Building && renderGrid[gridIndex].Amount == renderGrid[localIndex].Amount);

                    newPath = (short)(newPath | (canEnter ? (0x1 << i) : 0));
                }
                path[worldIndex] = newPath;
            }
            else if (prevTileType == TileType.Elevator)
            {
                if (levelIndex > 0)
                {
                    var level = SceneRefs.World.gridLevels[levelIndex - 1];
                    int _levelLocal = level.LocalIndexFromWorldIndex(worldIndex);
                    if (level.renderGrid[_levelLocal].TType == TileType.Elevator)
                    {
                        path[worldIndex] = (short)(path[worldIndex] & ~(1 << 4));
                        level.path[worldIndex] = (short)(level.path[worldIndex] & ~(1 << 5));
                    }
                }
                if (levelIndex < SceneRefs.World.LEVEL_COUNT - 1)
                {
                    var level = SceneRefs.World.gridLevels[levelIndex + 1];
                    int _levelLocal = level.LocalIndexFromWorldIndex(worldIndex);

                    if (level.renderGrid[_levelLocal].TType == TileType.Elevator)
                    {
                        path[worldIndex] = (short)(path[worldIndex] & ~(1 << 5));
                        level.path[worldIndex] = (short)(level.path[worldIndex] & ~(1 << 4));
                    }
                }
            }

            if (newTileType == TileType.Elevator)
            {
                if (levelIndex > 0)
                {
                    var level = SceneRefs.World.gridLevels[levelIndex - 1];
                    int _levelLocal = level.LocalIndexFromWorldIndex(worldIndex);

                    if (level.renderGrid[_levelLocal].TType == TileType.Elevator)
                    {
                        path[worldIndex] = (short)(path[worldIndex] | (1 << 4));
                        level.path[worldIndex] = (short)(level.path[worldIndex] | (1 << 5));
                    }
                }
                if (levelIndex < SceneRefs.World.LEVEL_COUNT - 1)
                {
                    var level = SceneRefs.World.gridLevels[levelIndex + 1];
                    int _levelLocal = level.LocalIndexFromWorldIndex(worldIndex);

                    if (level.renderGrid[_levelLocal].TType == TileType.Elevator)
                    {
                        path[worldIndex] = (short)(path[worldIndex] | (1 << 5));
                        level.path[worldIndex] = (short)(level.path[worldIndex] | (1 << 4));
                    }
                }
            }
        }

        static void ClearPath(
            LevelGridTile tile, 
            int worldIndex,
            //NativeArray<int> levelOffset,
            NativeArray<int> worldOffset,
            //NativeArray<LevelGridTile> renderGrid,
            NativeArray<short> path)
        {
            if (tile.TType == TileType.Building)
            {
                path[worldIndex] = 0;
            }

            for (int i = 0; i < 4; i++)
            {
                int _world = worldIndex - worldOffset[i];
                path[_world] = (short)(path[_world] & ~(short)(0x1 << i));
            }
        }
    }
}
