using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.Paths;
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

        public GridLevel(LevelComponent level, OverlayComponent overlay, int _levelNumber, int largestDimScaled)
        {
            // TODO
            if (_levelNumber == 0)
                Unlocked = true;

            levelNumber = _levelNumber;
            levelDimensions = level.dimensions;
            dimensionsScaled = level.dimensionsScaled;

            levelOffset = level.offsets;
            offsetScaled = level.offsetsScaled;

            sizeScaled = dimensionsScaled * dimensionsScaled;

            renderGrid = level.renderGrid;
            dirtyChunks = level.dirtyChunks;


            neighbourOffsets = level.neighbours; 
            /* 
            new NativeArray<int>(4, Allocator.Persistent);
            neighbourOffsets[0] = 1;
            neighbourOffsets[1] = dimensionsScaled;
            neighbourOffsets[2] = -1;
            neighbourOffsets[3] = -dimensionsScaled;
            */


            marked = overlay.markedTilesGrid;   // new NativeBitArray(largestDimScaled * largestDimScaled, Allocator.Persistent);
            overlayTiles = overlay.overlayGrid; // new NativeArray<OverlayData>(largestDimScaled * largestDimScaled, Allocator.Persistent);

            for (int i = 0; i < sizeScaled; i++)
            {
                if (renderGrid[i].TType == TileType.Road)
                {
                    RevealChanges.RevealRocks(i, level);
                }
            }
        }

        public void Dispose()
        {
            if (marked.IsCreated)
                marked.Dispose();

            if (overlayTiles.IsCreated)
                overlayTiles.Dispose();
        }
        
        public void UIUpdate(string property = "")
        {
            propertyChanged?.Invoke(this, new(property));
        }
    }
}
