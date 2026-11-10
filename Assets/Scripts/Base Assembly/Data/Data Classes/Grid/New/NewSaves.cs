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
        public int levelDimensions;
        public int levelOffset;
        public NativeArray<LevelGridTile> grid;
        public LevelGridSave(int _levelDimensions, int _levelOffset, NativeArray<LevelGridTile> grid)
        {
            levelDimensions = _levelDimensions;
            levelOffset = _levelOffset;
            this.grid = grid;
        }
    }

    public struct WorldSaveNew
    {
        public BuildsAndChunksSaveNew objectsSave;
        public LevelGridSave[] gridSave;


        public int largestDimension;
        public int chunkDimensions;
        public int levelCount;
        public NativeArray<short> pathSave;
        public int pathGridSize;
        public NativeArray<byte> pathRegions;
        public NativeBitArray regionList;
    }

    public struct BuildsAndChunksSaveNew
    {
        public BuildingNew[] buildings;
        public ChunkSave[] chunks;

        public BuildsAndChunksSaveNew(BuildingNew[] _buildings, ChunkSave[] _chunks)
        {
            buildings = _buildings;
            chunks = _chunks;
        }
    }
}
