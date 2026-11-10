using Rendering;
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
        public readonly int offsetScaled;
        public readonly int levelDimensions;
        public readonly int dimensionsScaled;
        public readonly int sizeScaled;
        public readonly int levelNumber;
        [CreateProperty] public bool Unlocked;
        public readonly NativeArray<int> neighbourOffsets;
        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;
        public NativeBitArray dirtyChunks;


        /// <summary>
        /// Marked rocks.
        /// </summary>
        public NativeBitArray marked;
        
        /// <summary>
        /// Color values, overlay like roads and digging.
        /// </summary>
        public NativeArray<OverlayData> overlayTiles;

        public GridLevel(LevelGridSave gridSave, int _levelNumber, int levelSize, NativeArray<short> gridPath, int largestDimScaled)
        {
            // TODO
            if (_levelNumber == 0)
                Unlocked = true;

            levelNumber = _levelNumber;
            levelDimensions = gridSave.levelDimensions;
            dimensionsScaled = levelDimensions * chunkDimensions;

            levelOffset = gridSave.levelOffset;
            offsetScaled = levelOffset * chunkDimensions;

            sizeScaled = dimensionsScaled * dimensionsScaled;

            renderGrid = gridSave.grid;
            path = new(gridPath, _levelNumber * levelSize, levelSize);


            neighbourOffsets = new NativeArray<int>(4, Allocator.Persistent);
            neighbourOffsets[0] = 1;
            neighbourOffsets[1] = dimensionsScaled;
            neighbourOffsets[2] = -1;
            neighbourOffsets[3] = -dimensionsScaled;

            marked = new NativeBitArray(largestDimScaled * largestDimScaled, Allocator.Persistent);
            overlayTiles = new NativeArray<OverlayData>(largestDimScaled * largestDimScaled, Allocator.Persistent);

            dirtyChunks = new NativeBitArray(levelDimensions * levelDimensions, Allocator.Persistent);

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

            int posY = i / dimensionsScaled;
            int posX = i % dimensionsScaled;
            posX = math.clamp(posX - range, 0, range);
            posY = math.clamp(posY - range, 0, range);

            int checkY = i / dimensionsScaled;
            int checkX = i % dimensionsScaled;
            checkY = math.clamp((dimensionsScaled+1) - checkY, 0, checkRange);
            checkX = math.clamp((dimensionsScaled+1) - checkX, 0, checkRange);



            //int checkIndex = i - range - range * levelDimensionsScaled;

            int checkIndex = i - posX - posY * dimensionsScaled;
            for (int y = 0; y < checkY; y++)
            {
                for (int x = 0; x < checkX; x++)
                {
                    LevelGridTile tile = renderGrid[checkIndex + x];
                    tile.hiddenTypeStyleAmount = ~1 & tile.hiddenTypeStyleAmount;
                    renderGrid[checkIndex + x] = tile;
                }
                checkIndex += dimensionsScaled;
            }
        }

        public bool IsRevealed(int i)
        {
            int checkRange = (range * 2) + 1;

            int posY = i / dimensionsScaled;
            int posX = i % dimensionsScaled;
            posX = math.clamp(posX - range, 0, range);
            posY = math.clamp(posY - range, 0, range);

            int checkY = i / dimensionsScaled;
            int checkX = i % dimensionsScaled;
            checkY = math.clamp((dimensionsScaled + 1) - checkY, 0, checkRange);
            checkX = math.clamp((dimensionsScaled + 1) - checkX, 0, checkRange);



            int checkIndex = i - posX - posY * dimensionsScaled;
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
                checkIndex += dimensionsScaled;
            }
            return false;
        }


        public void Dispose()
        {
            if (renderGrid.IsCreated)
                renderGrid.Dispose();

            if(dirtyChunks.IsCreated)
                dirtyChunks.Dispose();

            if (marked.IsCreated)
                marked.Dispose();

            if (overlayTiles.IsCreated)
                overlayTiles.Dispose();

            if (neighbourOffsets.IsCreated)
                neighbourOffsets.Dispose();
        }
        
        public void SetTileByWorldPos(int2 worldPosition, LevelGridTile tile) 
            => SetTile(Recalculation.LocalIndexFromWorldPos(worldPosition, offsetScaled, dimensionsScaled), tile);

        public void SetTile(int localIndex, LevelGridTile tile)
        {
            //UpdatePath(localIndex, tile);
            PathChanges.UpdatePath(
                localIndex, 
                levelNumber, 
                tile, 
                dimensionsScaled, 
                offsetScaled, 
                SceneRefs.World.offsetPathTableNeighbour, 
                neighbourOffsets,
                renderGrid, 
                SceneRefs.World.path);

            if (tile.Type != renderGrid[localIndex].Type)
                DirtyGridUtils.SetDirty(
                    dirtyChunks, 
                    dimensionsScaled, 
                    levelDimensions, 
                    localIndex, 
                    tile.TType == TileType.Wall ? 2 : 1);

            renderGrid[localIndex] = tile;

        }
/*
        private void UpdatePath(int localIndex, LevelGridTile tile)
        {
            TileType newTileType = tile.TType;
            TileType prevTileType = renderGrid[localIndex].TType;

            int worldIndex = Recalculation.WorldIndexFromLocalIndex(localIndex, offsetScaled, dimensionsScaled);
            var neighbours = SceneRefs.World.offsetPathTableNeighbour;

            if (newTileType == TileType.Road || newTileType == TileType.PassableBuilding || newTileType == TileType.Elevator)
            {
                SetPath(localIndex, newTileType, prevTileType, worldIndex, neighbours);
            }
            else
            {
                ClearPath(tile, worldIndex, neighbours);
            }
        }

        private void SetPath(int localIndex, TileType newTileType, TileType prevTileType, int worldIndex, int[] neighbours)
        {
            for (int i = 0; i < 4; i++)
            {
                int pathIndex = worldIndex + neighbours[i];
                int gridIndex = localIndex + neighbourOffsets[i];

                if (renderGrid[gridIndex].TType == TileType.Building)
                    continue;
                path[pathIndex] = (short)(path[pathIndex] | (short)(0x1 << i));
            }

            // was building
            HandleBuildingPath(localIndex, newTileType, prevTileType, worldIndex);
        }

        private void HandleBuildingPath(int localIndex, TileType newTileType, TileType prevTileType, int worldIndex)
        {
            if (prevTileType == TileType.Building)
            {
                short newPath = 0;
                for (int i = 0; i < 4; i++)
                {
                    int gridIndex = localIndex + neighbourOffsets[i];
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

        private void ClearPath(LevelGridTile tile, int worldIndex, int[] neighbours)
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
*/
        public void UIUpdate(string property = "")
        {
            propertyChanged?.Invoke(this, new(property));
        }

        public int LocalIndexFromWorldIndex(int world)
        {
            return Recalculation.LocalIndexFromWorldIndex(world, offsetScaled, dimensionsScaled);
        }

        public int LocalIndexFromWorldPos(int2 world)
        {
            world -= offsetScaled;

            return world.y * dimensionsScaled + world.x;
        }


        public void SetEntrance(int2 world, LevelRenderer levelRenderer, bool add)
        {
            int localIndex = LocalIndexFromWorldPos(world);
            var gridTile = renderGrid[localIndex];

            uint entranceCount = gridTile.otherData & 0x3;
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
                    overlayTiles[worldIndex] = overlayTiles[worldIndex].G((byte)(add ? 255 : 0));
                    Debug.Log($"Place Entrance on {world}");
                }
            }

            if (add)
            {
                gridTile.otherData++;
                renderGrid[localIndex] = gridTile;
            }
        }

        public void CleanPosAndUpdate(int gridIndex)
        {
            int worldPosition = Recalculation.WorldIndexFromLocalIndex(gridIndex, offsetScaled, dimensionsScaled);// WorldIndexFromLocalIndex(gridIndex);
            overlayTiles[worldPosition] = overlayTiles[worldPosition].R(0);
            marked.Set(worldPosition, false);
        }
    
    }
}
