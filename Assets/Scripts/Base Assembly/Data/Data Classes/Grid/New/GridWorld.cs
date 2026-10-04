using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World;
using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World.Buildings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Mathematics;
using Unity.Mathematics.Geometry;
using UnityEngine;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New
{
    [RequireComponent(typeof(LevelRenderer))]
    public class GridWorld : MonoBehaviour
    {
        public static int LEVEL_COUNT;
        [NonSerialized] public GridLevel[] gridLevels = null;
        public static object GetLevel(int i) => worldInstance.gridLevels[i];

        int currentLevelIndex;
        public static int CurrentLevelIndex => worldInstance.currentLevelIndex;
        public static GridLevel CurrentLevel => worldInstance.gridLevels[CurrentLevelIndex];
        
        LevelRenderer renderer;

        WorldBuildings buildings;

        public BuildingDataNew[] Buildings => buildings.buildings;

        [SerializeField] Mesh[] buildingMeshes;
        [SerializeField] Material buildingMaterial;
        [SerializeField] Material mat2;

        Action<int, int> onLevelChange;
        static GridWorld worldInstance;
        internal IEnumerable<Chunk> chunks;

        public void LoadGrid(WorldSaveNew saveToLoad)
        {
            worldInstance = this;
            onLevelChange = null;


            LEVEL_COUNT = saveToLoad.gridSave.Length;
            
            currentLevelIndex = 0;
            gridLevels = new GridLevel[LEVEL_COUNT];


            GridLevel.chunkDimensions = saveToLoad.chunkDimensions;
            for (int i = 0; i < LEVEL_COUNT; i++)
            {
                gridLevels[i] = new GridLevel(saveToLoad.gridSave[i], i);
            }

            renderer = GetComponent<LevelRenderer>();
            renderer.Init(LEVEL_COUNT, gridLevels);

            GetComponent<MouseListener>().Init(this);

            buildings = new WorldBuildings();// )GetComponent<>();
            buildings.LoadState(saveToLoad.objectsSave.buildings);

            renderer.AddRenderer(buildings.SetupRenderer(buildingMeshes, buildingMaterial, mat2));
        }

        public LevelGridTile GetTile(int x, int level, int y)
        {
            return gridLevels[level].renderGrid[gridLevels[level].totalDimensions.x * y + x];
        }

        public LevelGridTile GetActiveTile(int x, int y)
        {
            int index = gridLevels[0].totalDimensions.x * y + x;
            if (index < 0 || index >= gridLevels[0].renderGrid.Length)
                return LevelGridTile.OutsideWall;
            return gridLevels[0].renderGrid[gridLevels[0].totalDimensions.x * y + x];
        }


        public static void AttachChange(Action<int, int> attach) => worldInstance.onLevelChange += attach;
        public static void SetLevel(int i) => worldInstance.ChangeLevel(i);
        public void ChangeLevel(int i)
        {
            if (i == currentLevelIndex)
                return;

            onLevelChange?.Invoke(currentLevelIndex, i);

            currentLevelIndex = i;
        }

        private void OnDestroy()
        {
            buildings.Dispose();
            worldInstance = null;
        }

    }
}
