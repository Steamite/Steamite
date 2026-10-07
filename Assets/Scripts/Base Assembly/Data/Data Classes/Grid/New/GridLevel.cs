using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New
{
    public class GridLevel : IDisposable, IUpdatable
    {
        public static int chunkDimensions = 16;
        public NativeArray<LevelGridTile> renderGrid;
        public NativeArray<MovementAndStabilityData> movementAndStability;

        public readonly int totalDimensions;

        public readonly int levelOffset;
        public readonly int levelOffsetScaled;
        public readonly int levelDimensions;

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


            levelOffset = gridSave.levelOffset;
            levelOffsetScaled = levelOffset * chunkDimensions; 

            totalDimensions = chunkDimensions * levelDimensions;

            int tileCount = totalDimensions * totalDimensions;

            renderGrid = gridSave.grid;

            movementAndStability = new NativeArray<MovementAndStabilityData>(tileCount, Allocator.Persistent);

            // use a lookup array indexed by different tile Types
            // Construct node paths + stability
            for (int i = 0; i < tileCount; i++)
            {
                // TODO: reload objects based on save data
                // NO need to spawn any gameobjects store all in memory
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
            => SetTile(y * totalDimensions + x, tile);

        public void SetTile(int index, int tile)
            => movementAndStability[index] = new(tile);


        public void SetTile(int x, int y, LevelGridTile tile)
            => SetTile(y * totalDimensions + x, tile);

        public void SetTile(int index, LevelGridTile tile)
        {
            renderGrid[index] = tile;
            // RE-RENDER / mark dirty 
        }

        public void UIUpdate(string property = "")
        {
            propertyChanged?.Invoke(this, new(property));
        }
    }
}
