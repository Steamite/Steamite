using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Mathematics;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New
{
    public struct LevelGridSave
    {
        public int2 levelDimensions;
        public NativeArray<LevelGridTile> grid;

        public LevelGridSave(int2 _levelDimensions, NativeArray<LevelGridTile> grid)
        {
            levelDimensions = _levelDimensions;

            this.grid = grid;
        }
    }

    public class WorldSaveNew
    {
        public BuildsAndChunksSave objectsSave;
        public LevelGridSave[] gridSave;

        public int2 chunkDimensions;
    }
}
