using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New
{
    public static class PathChanges
    {
        public static void UpdatePath(
            int localIndex, 
            int levelIndex,
            LevelGridTile tile,
            int dimensionScaled,
            int offsetScaled,
            NativeArray<int> levelOffset,
            NativeArray<int> neighbourOffset,
            NativeArray<LevelGridTile> renderGrid,
            NativeArray<short> path)
        {
            TileType newTileType = tile.TType;
            TileType prevTileType = renderGrid[localIndex].TType;

            int worldIndex = Recalculation.WorldIndexFromLocal(localIndex, offsetScaled, dimensionScaled);
            
            if (newTileType == TileType.Road || newTileType == TileType.PassableBuilding || newTileType == TileType.Elevator)
            {
                SetPath(
                    localIndex, 
                    newTileType, 
                    prevTileType, 
                    worldIndex, 
                    levelIndex, 
                    levelOffset, 
                    neighbourOffset, 
                    renderGrid, 
                    path);
            }
            else
            {
                ClearPath(
                    tile,
                    worldIndex,
                    levelOffset,
                    path);
            }
        }

        static void SetPath(
            int localIndex, 
            TileType newTileType, 
            TileType prevTileType, 
            int worldIndex, 
            int levelIndex,
            NativeArray<int> levelOffset,
            NativeArray<int> neighbourOffset,
            NativeArray<LevelGridTile> renderGrid,
            NativeArray<short> path)
        {
            for (int i = 0; i < 4; i++)
            {
                // TODO: fix properly tomorrow
                int pathIndex = worldIndex - levelOffset[i];
                int gridIndex = localIndex + neighbourOffset[i];

                if (renderGrid[gridIndex].TType == TileType.Building)
                    continue;
                path[pathIndex] = (short)(path[pathIndex] | (short)(0x1 << i));
            }

            // was building
            HandleBuildingPath(localIndex, newTileType, prevTileType, worldIndex, levelIndex, levelOffset, neighbourOffset, renderGrid, path);
        }

        static void HandleBuildingPath(
            int localIndex, 
            TileType newTileType, 
            TileType prevTileType, 
            int worldIndex,
            int levelIndex,
            NativeArray<int> levelOffset,
            NativeArray<int> neighbourOffset,
            NativeArray<LevelGridTile> renderGrid,
            NativeArray<short> path)
        {
            if (prevTileType == TileType.Building)
            {
                short newPath = 0;
                for (int i = 0; i < 4; i++)
                {
                    int gridIndex = localIndex + levelOffset[i];
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
            NativeArray<int> neighbourOffset,
            //NativeArray<LevelGridTile> renderGrid,
            NativeArray<short> path)
        {
            if (tile.TType == TileType.Building)
            {
                path[worldIndex] = 0;
            }

            for (int i = 0; i < 4; i++)
            {
                int _world = worldIndex + neighbourOffset[i];
                path[_world] = (short)(path[_world] & ~(short)(0x1 << i));
            }
        }
    }
}
