using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World.Buildings;
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
        public BuildsAndChunksSaveNew objectsSave;
        public LevelGridSave[] gridSave;

        public int2 chunkDimensions;
    }

    public class BuildsAndChunksSaveNew
    {
        public BuildingDataNew[] buildings;
        public ChunkSave[] chunks;

        public BuildsAndChunksSaveNew(BuildingDataNew[] _buildings, ChunkSave[] _chunks)
        {
            buildings = _buildings;
            chunks = _chunks;
        }
    }
}
