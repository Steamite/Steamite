using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Collections;
using Unity.Mathematics;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New
{
    public struct GridLevel : IDisposable
    {
        public static int2 chunkDimensions = new(16, 16);
        public NativeArray<LevelGridTile> renderGrid;
        NativeArray<MovementAndStabilityData> movementAndStability;
        readonly int size;
        public readonly int2 totalDimensions;
        public readonly int2 levelDimensions;

        public readonly int levelNumber;

        public GridLevel(LevelGridSave gridSave, int _levelNumber)
        {
            levelNumber = _levelNumber;
            levelDimensions = gridSave.levelDimensions;

            totalDimensions.x = chunkDimensions.x * levelDimensions.x;
            totalDimensions.y = chunkDimensions.y * levelDimensions.y;

            size = totalDimensions.x * totalDimensions.y;

            renderGrid = gridSave.grid;

            movementAndStability = new NativeArray<MovementAndStabilityData>(size, Allocator.Persistent);

            // use a lookup array indexed by different tile Types
            // Construct node paths + stability
            for (int i = 0; i < size; i++)
            {

                // TODO: Spawn objects based on save data
                // Maybe create methods for creating the data, or completly change the data?
                // Will need to change saving
                // Change Road saving 
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
            => SetTile(y * totalDimensions.x + x, tile);

        public void SetTile(int index, int tile)
            => movementAndStability[index] = new(tile);


        public void SetTile(int x, int y, LevelGridTile tile)
            => SetTile(y * totalDimensions.x + x, tile);

        public void SetTile(int index, LevelGridTile tile)
        {
            renderGrid[index] = tile;
            // RE-RENDER / mark dirty 
        }
    }
}
