using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New
{
    public class GridLevel : IDisposable, IUpdatable
    {
        public static int chunkDimensions = 16;
        public NativeArray<LevelGridTile> renderGrid;
        /// <summary>
        /// Indexed by world position, not local!
        /// 0 - Left <br/>
        /// 1 - Backwards <br/>
        /// 2 - Right <br/>
        /// 3 - Forward <br/>
        /// 4 - Down <br/>
        /// 5 - Up <br/>
        /// </summary>
        public NativeSlice<short> path;
        //public NativeArray<MovementAndStabilityData> movementAndStability;

        public readonly int levelOffset;
        public readonly int levelOffsetScaled;
        public readonly int levelDimensions;
        public readonly int levelDimensionsScaled;
        public readonly int sizeScaled;
        public readonly int levelNumber;
        [CreateProperty] public bool Unlocked;
        public readonly int[] pathNeighbourTable;
        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

        public GridLevel(LevelGridSave gridSave, int _levelNumber, int levelSize, NativeArray<short> gridPath)
        {
            // TODO
            if (_levelNumber == 0)
                Unlocked = true;

            levelNumber = _levelNumber;
            levelDimensions = gridSave.levelDimensions;
            levelDimensionsScaled = levelDimensions * chunkDimensions;

            levelOffset = gridSave.levelOffset;
            levelOffsetScaled = levelOffset * chunkDimensions;

            sizeScaled = levelDimensionsScaled * levelDimensionsScaled;

            renderGrid = gridSave.grid;
            path = new(gridPath, _levelNumber * levelSize, levelSize);

            pathNeighbourTable = new int[] { 1, levelDimensionsScaled, -1, -levelDimensionsScaled};
            //movementAndStability = new NativeArray<MovementAndStabilityData>(tileCount, Allocator.Persistent);

            // use a lookup array indexed by different tile Types
            // Construct node paths + stability

            for (int i = 0; i < sizeScaled; i++)
            {

                if (renderGrid[i].TType == TileType.Road)
                {
                    RevealRocks(i);
                }
            }
        }

        int range = 2;

        public void RevealRocks(int i)
        {
            int checkRange = (range * 2) + 1;

            int posY = i / levelDimensionsScaled;
            int posX = i % levelDimensionsScaled;
            posX = math.clamp(posX - range, 0, range);
            posY = math.clamp(posY - range, 0, range);

            int checkY = i / levelDimensionsScaled;
            int checkX = i % levelDimensionsScaled;
            checkY = math.clamp((levelDimensionsScaled+1) - checkY, 0, checkRange);
            checkX = math.clamp((levelDimensionsScaled+1) - checkX, 0, checkRange);



            //int checkIndex = i - range - range * levelDimensionsScaled;

            int checkIndex = i - posX - posY * levelDimensionsScaled;
            for (int y = 0; y < checkY; y++)
            {
                for (int x = 0; x < checkX; x++)
                {
                    LevelGridTile tile = renderGrid[checkIndex + x];
                    tile.hiddenTypeStyleAmount = ~1 & tile.hiddenTypeStyleAmount;
                    renderGrid[checkIndex + x] = tile;
                }
                checkIndex += levelDimensionsScaled;
            }
        }

        public bool IsRevealed(int i)
        {
            int checkRange = (range * 2) + 1;

            int posY = i / levelDimensionsScaled;
            int posX = i % levelDimensionsScaled;
            posX = math.clamp(posX - range, 0, range);
            posY = math.clamp(posY - range, 0, range);

            int checkY = i / levelDimensionsScaled;
            int checkX = i % levelDimensionsScaled;
            checkY = math.clamp((levelDimensionsScaled + 1) - checkY, 0, checkRange);
            checkX = math.clamp((levelDimensionsScaled + 1) - checkX, 0, checkRange);



            int checkIndex = i - posX - posY * levelDimensionsScaled;
            for (int y = 0; y < checkY; y++)
            {
                for (int x = 0; x < checkX; x++)
                {
                    if (checkIndex + x == i)
                        continue;

                    LevelGridTile tile = renderGrid[checkIndex + x];
                    if (tile.TType == TileType.Road)
                        return true;
                }
                checkIndex += levelDimensionsScaled;
            }
            return false;
        }


        public void Dispose()
        {
            if (renderGrid.IsCreated)
                renderGrid.Dispose();
        }
        
        public void SetTileByWorldPos(int2 worldPosition, LevelGridTile tile) 
            => SetTile(LocalIndexFromWorldPos(worldPosition), tile);

        public LevelGridTile GetTile(int pos) => renderGrid[pos];

        public void SetTile(int localIndex, LevelGridTile tile)
        {
            UpdatePath(localIndex, tile);

            renderGrid[localIndex] = tile;
        }

        private void UpdatePath(int localIndex, LevelGridTile tile)
        {
            TileType newTileType = tile.TType;
            TileType prevTileType = renderGrid[localIndex].TType;

            int worldIndex = WorldIndexFromLocalIndex(localIndex);
            var neighbours = SceneRefs.World.offsetPathTableNeighbour;

            if (newTileType == TileType.Road || newTileType == TileType.PassableBuilding || newTileType == TileType.Elevator)
            {
                for (int i = 0; i < 4; i++)
                {
                    int pathIndex = worldIndex + neighbours[i];
                    int gridIndex = localIndex + pathNeighbourTable[i];

                    if (renderGrid[gridIndex].TType == TileType.Building)
                        continue;
                    path[pathIndex] = (short)(path[pathIndex] | (short)(0x1 << i));
                }

                // was building
                if (prevTileType == TileType.Building)
                {
                    short newPath = 0;
                    for (int i = 0; i < 4; i++)
                    {
                        int gridIndex = localIndex + pathNeighbourTable[i];
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
                    if (levelNumber > 0)
                    {
                        var level = SceneRefs.World.gridLevels[levelNumber - 1];
                        int _levelLocal = level.LocalIndexFromWorldIndex(worldIndex);
                        if (level.renderGrid[_levelLocal].TType == TileType.Elevator)
                        {
                            path[worldIndex] = (short)(path[worldIndex] & ~(1 << 4));
                            level.path[worldIndex] = (short)(level.path[worldIndex] & ~(1 << 5));
                        }
                    }
                    if (levelNumber < SceneRefs.World.LEVEL_COUNT - 1)
                    {
                        var level = SceneRefs.World.gridLevels[levelNumber + 1];
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
                    if (levelNumber > 0)
                    {
                        var level = SceneRefs.World.gridLevels[levelNumber - 1];
                        int _levelLocal = level.LocalIndexFromWorldIndex(worldIndex);

                        if (level.renderGrid[_levelLocal].TType == TileType.Elevator)
                        {
                            path[worldIndex] = (short)(path[worldIndex] | (1 << 4));
                            level.path[worldIndex] = (short)(level.path[worldIndex] | (1 << 5));
                        }
                    }
                    if (levelNumber < SceneRefs.World.LEVEL_COUNT - 1)
                    {
                        var level = SceneRefs.World.gridLevels[levelNumber + 1];
                        int _levelLocal = level.LocalIndexFromWorldIndex(worldIndex);

                        if (level.renderGrid[_levelLocal].TType == TileType.Elevator)
                        {
                            path[worldIndex] = (short)(path[worldIndex] | (1 << 5));
                            level.path[worldIndex] = (short)(level.path[worldIndex] | (1 << 4));
                        }
                    }
                }
            }
            else
            {
                if (tile.TType == TileType.Building)
                {
                    path[worldIndex] = 0;
                }

                for (int i = 0; i < 4; i++)
                {
                    int _world = worldIndex + neighbours[i];
                    path[_world] = (short)(path[_world] & ~(short)(0x1 << i));
                }
            }
        }

        public void UIUpdate(string property = "")
        {
            propertyChanged?.Invoke(this, new(property));
        }

        public int WorldIndexFromLocalIndex(int gridIndex)
        {
            int x = gridIndex % levelDimensionsScaled + levelOffsetScaled;
            int y = gridIndex / levelDimensionsScaled + levelOffsetScaled;


            int original = x + y * (levelDimensionsScaled + (2 * levelOffsetScaled));
            return original;
        }

        public int2 WorldPosFromLocalIndex(int localIndex)
        {
            int x = localIndex % levelDimensionsScaled + levelOffsetScaled;
            int y = localIndex / levelDimensionsScaled + levelOffsetScaled;

            return new(x, y);
        }

        public int LocalIndexFromWorldIndex(int world)
        {
            int x = (world % (levelDimensionsScaled + (2 * levelOffsetScaled))) - levelOffsetScaled;
            int y = (world / (levelDimensionsScaled + (2 * levelOffsetScaled))) - levelOffsetScaled;
            
            return y * levelDimensionsScaled + x;
        }

        public int LocalIndexFromWorldPos(int2 world)
        {
            world -= levelOffsetScaled;

            return world.y * levelDimensionsScaled + world.x;
        }


        public void SetEntrance(int2 world, LevelRenderer levelRenderer, bool add)
        {
            int localIndex = LocalIndexFromWorldPos(world);
            var gridTile = renderGrid[localIndex];

            int entranceCount = gridTile.otherData & 0x3;
            if (!add)
            {
                entranceCount--;
                gridTile.otherData--;
                renderGrid[localIndex] = gridTile;
            }

            if (gridTile.TType == TileType.Road)
            {
                if (entranceCount == 0) 
                { 
                    int worldIndex = SceneRefs.World.GetWorldIndex(world);
                    levelRenderer.levelOverlayGrid[worldIndex] = 
                        levelRenderer.levelOverlayGrid[worldIndex].G((byte)(add ? 255 : 0));
                    Debug.Log($"Place Entrance on {world}");
                }
            }

            if (add)
            {
                gridTile.otherData++;
                renderGrid[localIndex] = gridTile;
            }
        }
    }
}
