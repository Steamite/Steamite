using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New
{
    public static class PathChanges
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="localIndex"></param>
        /// <param name="levelIndex"></param>
        /// <param name="newTile"></param>
        /// <param name="worldOffset">Offset using largest dim scaled</param>
        /// <param name="path">Path for the whole world.</param>
        
        public static void UpdatePath(
            int localIndex, 
            int levelIndex,
            LevelGridTile newTile,
            WorldData world)
        {
            LevelData levelData = world.levels[levelIndex];

            TileType newTileType = newTile.TType;
            TileType prevTileType = levelData.renderGrid[localIndex].TType;


            int globalIndex = Recalculation.GlobalIndexFromLocalIndex(
                localIndex,
                levelData.offsetScaled,
                levelData.dimScaled,
                levelData.startIndex);

            if (newTileType == TileType.Road || newTileType == TileType.PassableBuilding || newTileType == TileType.Elevator)
            {
                OpenPath(
                    localIndex,
                    globalIndex, 
                    world.neighbourOffset,
                    levelData.neighbourOffsetTable,
                    levelData.renderGrid, 
                    world.path);

                /*HandleBuildingPath(
                    localIndex, 
                    newTileType, 
                    prevTileType, 
                    globalIndex, 
                    levelIndex, 
                    levelOffset, 
                    renderGrid, 
                    path);*/
            }
            else
            {
                if (newTile.TType == TileType.Building)
                    world.path[globalIndex] = 0;

                BlockPath(
                    globalIndex,
                    world.neighbourOffset,
                    world.path);
            }
        }

        static void OpenPath(
            int localIndex,
            int globalPosition,
            NativeArray<int> worldOffset,
            NativeArray<int> levelOffset,
            NativeSlice<LevelGridTile> renderGrid,
            NativeArray<short> path)
        {
            for (int i = 0; i < 4; i++)
            {
                int gridIndex = localIndex + levelOffset[i];
                int pathIndex = globalPosition - worldOffset[i];

                if (renderGrid[gridIndex].TType == TileType.Building)
                    continue;
                path[pathIndex] = (short)(path[pathIndex] | (short)(0x1 << i));
            }
        }

        static void BlockPath(
            int globalPosition,
            NativeArray<int> worldOffset,
            NativeArray<short> path)
        {
            for (int i = 0; i < 4; i++)
            {
                int _world = globalPosition - worldOffset[i];
                path[_world] = (short)(path[_world] & ~(short)(0x1 << i));
            }
        }

        static void HandleBuildingPath(
            int localIndex, 
            TileType newTileType, 
            TileType prevTileType, 
            int globalPosition,
            int levelIndex,
            WorldData worldData/*,
            NativeArray<LevelData> levels,
            NativeArray<int> neighbourOffset,
            NativeArray<short> path*/)
        {
            var level = worldData.levels[levelIndex];
            if (prevTileType == TileType.Building)
            {
                short newPath = 0;
                for (int i = 0; i < 4; i++)
                {
                    int gridIndex = localIndex - worldData.neighbourOffset[i];
                    bool canEnter =
                        level.renderGrid[gridIndex].TType == TileType.Road
                        ||
                        (level.renderGrid[gridIndex].TType > TileType.Building && level.renderGrid[gridIndex].Amount == level.renderGrid[localIndex].Amount);

                    newPath = (short)(newPath | (canEnter ? (0x1 << i) : 0));
                }
                worldData.path[globalPosition] = newPath;
            }
            else if (prevTileType == TileType.Elevator)
            {
                if (levelIndex > 0)
                {
                    TestLowerElevator(globalPosition, levelIndex, worldData, level);
                }
                if (levelIndex < SceneRefs.World.LEVEL_COUNT - 1)
                {
                    var level = SceneRefs.World.gridLevels[levelIndex + 1];
                    int _levelLocal = level.LocalIndexFromWorldIndex(globalPosition);

                    if (level.renderGrid[_levelLocal].TType == TileType.Elevator)
                    {
                        path[globalPosition] = (short)(path[globalPosition] & ~(1 << 5));
                        level.path[globalPosition] = (short)(level.path[globalPosition] & ~(1 << 4));
                    }
                }
            }

            if (newTileType == TileType.Elevator)
            {
                if (levelIndex > 0)
                {
                    var level = SceneRefs.World.gridLevels[levelIndex - 1];
                    int _levelLocal = level.LocalIndexFromWorldIndex(globalPosition);

                    if (level.renderGrid[_levelLocal].TType == TileType.Elevator)
                    {
                        path[globalPosition] = (short)(path[globalPosition] | (1 << 4));
                        level.path[globalPosition] = (short)(level.path[globalPosition] | (1 << 5));
                    }
                }
                if (levelIndex < SceneRefs.World.LEVEL_COUNT - 1)
                {
                    var level = SceneRefs.World.gridLevels[levelIndex + 1];
                    int _levelLocal = level.LocalIndexFromWorldIndex(globalPosition);

                    if (level.renderGrid[_levelLocal].TType == TileType.Elevator)
                    {
                        path[globalPosition] = (short)(path[globalPosition] | (1 << 5));
                        level.path[globalPosition] = (short)(level.path[globalPosition] | (1 << 4));
                    }
                }
            }
        }

        private static void TestLowerElevator(int globalPosition, int levelIndex, WorldData worldData, LevelData level, bool up)
        {
            var _level = worldData.levels[levelIndex + (up ? 1 : -1)];
            int worldPosition = globalPosition % worldData.largestLevelSize;
            int nextIndex = Recalculation.LocalIndexFromWorldIndex(
                worldPosition,
                _level.offsetScaled,
                _level.dimScaled);

            int nextGlobalPosition = globalPosition + (up ? 1 : -1) * worldData.largestLevelSize;

            if (level.renderGrid[nextIndex].TType == TileType.Elevator)
            {
                worldData.path[globalPosition] = (short)(worldData.path[globalPosition] & ~(1 << 4));
                worldData.path[nextGlobalPosition] = (short)(worldData.path[nextGlobalPosition] & ~(1 << 5));
            }
        }
    }
}
