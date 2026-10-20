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
        public NativeArray<MovementAndStabilityData> movementAndStability;

        public readonly int levelOffset;
        public readonly int levelOffsetScaled;
        public readonly int levelDimensions;
        public readonly int levelDimensionsScaled;

        public readonly int levelNumber;
        [CreateProperty] public bool Unlocked;

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

            int tileCount = levelDimensionsScaled * levelDimensionsScaled;

            renderGrid = gridSave.grid;

            movementAndStability = new NativeArray<MovementAndStabilityData>(tileCount, Allocator.Persistent);

            // use a lookup array indexed by different tile Types
            // Construct node paths + stability

            for (int i = 0; i < tileCount; i++)
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

        public void Dispose()
        {
            if(renderGrid.IsCreated)
                renderGrid.Dispose();

            if (movementAndStability.IsCreated)
                movementAndStability.Dispose();

            if (movementAndStability.IsCreated)
                movementAndStability.Dispose();
        }

        public void SetTile(int x, int y, int tile)
            => SetTile(y * levelDimensionsScaled + x, tile);

        public void SetTile(int index, int tile)
            => movementAndStability[index] = new(tile);


        public void SetTile(int x, int y, LevelGridTile tile)
            => SetTile(y * levelDimensionsScaled + x, tile);
        public void SetTileByWorldPos(int2 pos, LevelGridTile tile)
        {
            pos -= levelOffsetScaled;
            SetTile(pos.y * levelDimensionsScaled + pos.x, tile);
        }
        public void SetTile(int index, LevelGridTile tile)
        {
            renderGrid[index] = tile;
        }

        public void UIUpdate(string property = "")
        {
            propertyChanged?.Invoke(this, new(property));
        }
    }
}
