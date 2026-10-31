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
        public NativeArray<short> path;
        //public NativeArray<MovementAndStabilityData> movementAndStability;

        public readonly int levelOffset;
        public readonly int levelOffsetScaled;
        public readonly int levelDimensions;
        public readonly int levelDimensionsScaled;
        public readonly int sizeScaled;
        public readonly int levelNumber;
        [CreateProperty] public bool Unlocked;
        public readonly int[] offsetPathTableNeighbour;
        public readonly int[] offsetPathTableSelf;
        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

        public GridLevel(LevelGridSave gridSave, int _levelNumber)
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
            path = gridSave.path;
            offsetPathTableNeighbour = new int[] { 1, levelDimensionsScaled, -1, -levelDimensionsScaled};
            offsetPathTableSelf = new int[] { -1, -levelDimensionsScaled, 1, levelDimensionsScaled};
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

            if (path.IsCreated)
                path.Dispose();
        }
        
        public void SetTileByWorldPos(int2 worldPosition, LevelGridTile tile) 
            => SetTile(LocalIndexFromWorldPos(worldPosition), tile);

        public LevelGridTile GetTile(int pos) => renderGrid[pos];

        public void SetTile(int pos, LevelGridTile tile)
        {
            if (tile.TType == TileType.Road || tile.TType == TileType.PassableBuilding)
            {
                for (int i = 0; i < 4; i++)
                {
                    int p = pos + offsetPathTableNeighbour[i];
                    if (renderGrid[p].TType == TileType.Building)
                        continue;
                    path[p] = (short)(path[p] | (short)(0x1 << i));
                }

                // was building
                if (renderGrid[pos].TType == TileType.Building)
                {
                    short newPath = 0;
                    for (int i = 0; i < 4; i++)
                    {
                        int p = pos + offsetPathTableSelf[i];

                        bool canEnter = 
                            renderGrid[p].TType == TileType.Road 
                            || 
                            (renderGrid[p].TType == TileType.Building && renderGrid[p].Amount == renderGrid[pos].Amount);

                        newPath = (short)(newPath | (canEnter ? (0x1 << i) : 0 ));
                    }
                    path[pos] = newPath;
                }
                // check buildings or save the entrance in all data
            }
            else
            {
                if(tile.TType == TileType.Building)
                {
                    path[pos] = 0;
                }

                for (int i = 0; i < 4; i++)
                {
                    int p = pos + offsetPathTableNeighbour[i];
                    path[p] = (short)(path[p] & ~(short)(0x1 << i));
                }
            }
            renderGrid[pos] = tile;
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
