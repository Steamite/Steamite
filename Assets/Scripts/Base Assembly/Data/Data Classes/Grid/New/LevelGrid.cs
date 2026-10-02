using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Mathematics;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New
{
    public struct LevelGrid
    {
        public int2 levelDimensions;
        public NativeArray<LevelGridTile> grid;

        public LevelGrid(int2 _levelDimensions, NativeArray<LevelGridTile> grid)
        {
            levelDimensions = _levelDimensions;

            this.grid = grid;
        }
    }

    public struct WorldSaveNew
    {
        public BuildsAndChunksSave objectsSave;
        public LevelGrid[] gridSave;

        public int2 chunkDimensions;
    }
    

}
