using Unity.Burst;
using Unity.Collections;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New
{
    [BurstCompile]
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
        [BurstCompile]
        public static void UpdatePath(
            int localIndex,
            int levelIndex,
            in LevelGridTile newTile,
            ref WorldComponent worldData,
            in NativeArray<LevelComponent> levels)
        {
            LevelComponent levelData = levels[levelIndex];

            TileType newTileType = newTile.TType;
            TileType prevTileType = levelData.renderGrid[localIndex].TType;


            int globalIndex = Recalculation.GlobalIndexFromLocalIndex(
                localIndex,
                levelData.offsetsScaled,
                levelData.dimensionsScaled,
                levelIndex);

            if (newTileType == TileType.Road || newTileType == TileType.PassableBuilding || newTileType == TileType.Elevator)
            {
                OpenPath(
                    localIndex,
                    globalIndex,
                    worldData.offsets,
                    levelData.neighbours,
                    levelData.renderGrid,
                    ref worldData.path);

                HandleBuildingChanges(localIndex, newTileType, prevTileType, globalIndex, levelIndex, ref worldData, in levels);
            }
            else
            {
                if (newTile.TType == TileType.Building)
                    worldData.path[globalIndex] = 0;

                BlockPath(
                    globalIndex,
                    worldData.offsets,
                    ref worldData.path);
            }
        }

        [BurstCompile]
        static void OpenPath(
            int localIndex,
            int globalPosition,
            in NativeArray<int> worldOffset,
            in NativeArray<int> levelOffset,
            in NativeSlice<LevelGridTile> renderGrid,
            ref NativeArray<short> path)
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

        [BurstCompile]
        static void BlockPath(
            int globalPosition,
            in NativeArray<int> worldOffset,
            ref NativeArray<short> path)
        {
            for (int i = 0; i < 4; i++)
            {
                int _world = globalPosition - worldOffset[i];
                path[_world] = (short)(path[_world] & ~(short)(0x1 << i));
            }
        }

        [BurstCompile]
        static void HandleBuildingChanges(
            int localIndex,
            TileType newTileType,
            TileType prevTileType,
            int globalPosition,
            int levelIndex,
            ref WorldComponent worldData,
            in NativeArray<LevelComponent> levels)
        {
            var level = levels[levelIndex];
            if (prevTileType == TileType.Building)
            {
                short newPath = 0;
                for (int i = 0; i < 4; i++)
                {
                    int gridIndex = localIndex - worldData.offsets[i];
                    bool canEnter =
                        level.renderGrid[gridIndex].TType == TileType.Road
                        ||
                        (level.renderGrid[gridIndex].TType > TileType.Building && level.renderGrid[gridIndex].Amount == level.renderGrid[localIndex].Amount);

                    newPath = (short)(newPath | (short)(canEnter ? (0x1 << i) : 0));
                }
                worldData.path[globalPosition] = newPath;
            }
            else if (prevTileType == TileType.Elevator)
            {
                if (levelIndex > 0)
                {
                    RemoveElevator(globalPosition, levelIndex, ref worldData, in levels, false);
                }
                if (levelIndex < levels.Length - 1)
                {
                    RemoveElevator(globalPosition, levelIndex, ref worldData, in levels, true);
                }
            }

            if (newTileType == TileType.Elevator)
            {
                if (levelIndex > 0)
                {
                    AddElevator(globalPosition, levelIndex, ref worldData, in levels, false);
                }
                if (levelIndex < levels.Length - 1)
                {
                    AddElevator(globalPosition, levelIndex, ref worldData, in levels, true);
                }
            }
        }

        [BurstCompile]
        private static void RemoveElevator(int globalPosition, int levelIndex, ref WorldComponent worldData, in NativeArray<LevelComponent> levels, bool up)
        {
            var _level = levels[levelIndex + (up ? 1 : -1)];
            int worldPosition = globalPosition % worldData.largestGridSize;
            /*int nextIndex = Recalculation.LocalIndexFromWorldIndex(
                worldPosition,
                _level.offsetScaled,
                _level.dimScaled);*/

            int nextGlobalPosition = globalPosition + (up ? 1 : -1) * worldData.largestGridSize;
            /*
                        if (_level.renderGrid[nextIndex].TType == TileType.Elevator)
                        {*/
            worldData.path[globalPosition] = (short)(worldData.path[globalPosition] & ~(1 << (up ? 5 : 4)));
            worldData.path[nextGlobalPosition] = (short)(worldData.path[nextGlobalPosition] & ~(1 << (up ? 4 : 5)));
            //}
        }

        [BurstCompile]
        private static void AddElevator(int globalPosition, int levelIndex, ref WorldComponent worldData, in NativeArray<LevelComponent> levels, bool up)
        {
            var _level = levels[levelIndex + (up ? 1 : -1)];
            int worldPosition = globalPosition % worldData.largestGridSize;
            int nextIndex = Recalculation.LocalIndexFromWorldIndex(
                worldPosition,
                _level.offsetsScaled,
                _level.dimensionsScaled);

            int nextGlobalPosition = globalPosition + (up ? 1 : -1) * worldData.largestGridSize;

            if (_level.renderGrid[nextIndex].TType == TileType.Elevator)
            {
                worldData.path[globalPosition] = (short)(worldData.path[globalPosition] | (short)(1 << (up ? 5 : 4)));
                worldData.path[nextGlobalPosition] = (short)(worldData.path[nextGlobalPosition] | (short)(1 << (up ? 4 : 5)));
            }
        }
    }
}
