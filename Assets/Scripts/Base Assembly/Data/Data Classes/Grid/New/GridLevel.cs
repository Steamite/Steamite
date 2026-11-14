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
        public readonly static int chunkDimensions = 16;
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

        public GridLevel(LevelComponent gridSave, int _levelNumber, int levelSize, NativeArray<short> gridPath, int largestDimScaled)
        {
            // TODO
            if (_levelNumber == 0)
                Unlocked = true;

            levelNumber = _levelNumber;
            levelDimensions = gridSave.dimensions;
            dimensionsScaled = gridSave.dimensionsScaled;

            levelOffset = gridSave.offsets;
            offsetScaled = gridSave.offsetsScaled;

            sizeScaled = dimensionsScaled * dimensionsScaled;

            renderGrid = gridSave.renderGrid;
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
        
        public void UIUpdate(string property = "")
        {
            propertyChanged?.Invoke(this, new(property));
        }

        /// <summary>
        /// Entrance overlay Tile
        /// </summary>
        /// <param name="world"></param>
        /// <param name="levelRenderer"></param>
        /// <param name="add"></param>
        public void SetEntrance(int2 world, LevelRenderer levelRenderer, bool add)
        {
            int localIndex = Recalculation.LocalIndexFromWorldPos(world, offsetScaled, dimensionsScaled);
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
