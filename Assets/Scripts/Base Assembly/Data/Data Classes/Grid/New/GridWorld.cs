using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World;
using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World.Buildings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Mathematics;
using Unity.Mathematics.Geometry;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New
{
    [RequireComponent(typeof(LevelRenderer))]
    public class GridWorld : MonoBehaviour
    {
        public int LEVEL_COUNT;
        [NonSerialized] public GridLevel[] gridLevels = null;
        public GridLevel GetLevel(int i) => gridLevels[i];

        int currentLevelIndex;
        public int CurrentLevelIndex => currentLevelIndex;
        GridLevel currentLevel;
        
        LevelRenderer renderer;

        WorldBuildings buildings;

        public BuildingNew[] Buildings => buildings.buildings;



        public int CenterScaled;
        [NonSerialized] public int LargestLevelDimensions;

        [SerializeField] Mesh[] buildingMeshes;
        [SerializeField] Material buildingMaterial;
        [SerializeField] Material mat2;

        Action<int, int> onLevelChange;
        internal IEnumerable<Chunk> chunks;
        
        

        public void LoadGrid(WorldSaveNew saveToLoad)
        {
            onLevelChange = null;
            currentLevelIndex = -1;

            LEVEL_COUNT = saveToLoad.gridSave.Length;
            
            gridLevels = new GridLevel[LEVEL_COUNT];

            GridLevel.chunkDimensions = saveToLoad.chunkDimensions;
            LargestLevelDimensions = saveToLoad.largestDimension;
            
            for (int i = 0; i < LEVEL_COUNT; i++)
            {
                gridLevels[i] = new GridLevel(saveToLoad.gridSave[i], i);
            }

            CenterScaled = (LargestLevelDimensions / 2) * GridLevel.chunkDimensions;

            buildings = new WorldBuildings();// )GetComponent<>();
            buildings.LoadState(saveToLoad.objectsSave.buildings);


            renderer = GetComponent<LevelRenderer>();
            renderer.AddRenderer(buildings.SetupRenderer(buildingMeshes, buildingMaterial, mat2));            
            renderer.Init(LEVEL_COUNT, gridLevels);

            SetLevel(0);
        }

        public LevelGridTile GetTile(int x, int level, int y)
        {
            return gridLevels[level].renderGrid[gridLevels[level].totalDimensions * y + x];
        }


        public LevelGridTile GetActiveTile(int x, int y)
        {
            x -= currentLevel.levelOffsetScaled;
            y -= currentLevel.levelOffsetScaled;
            //Debug.Log($"{x}, {y}, {currentLevel.levelOffsetScaled}");
            
            int index = gridLevels[0].totalDimensions * y + x;
            if (index < 0 || index >= gridLevels[0].renderGrid.Length)
                return LevelGridTile.OutsideWall;
            return gridLevels[0].renderGrid[index];
        }

        public LevelGridTile GetActiveTile(int x, int y, out int index)
        {
            x -= currentLevel.levelOffsetScaled;
            y -= currentLevel.levelOffsetScaled;
            //Debug.Log($"{x}, {y}, {currentLevel.levelOffsetScaled}");

            index = gridLevels[0].totalDimensions * y + x;
            if (index < 0 || index >= gridLevels[0].renderGrid.Length)
                return LevelGridTile.OutsideWall;
            return gridLevels[0].renderGrid[index];
        }





        public void AttachChange(Action<int, int> attach) => onLevelChange += attach;
        public void SetLevel(int i)
        {
            if (i == currentLevelIndex)
                return;

            int x = currentLevelIndex;
            currentLevelIndex = i;
            currentLevel = gridLevels[i];

            onLevelChange?.Invoke(x, i);
        }

        private void OnDestroy()
        {
            buildings.Dispose();
            foreach (var level in gridLevels)
            {
                level.Dispose();
            }
        }

        public bool IsUnlocked(int i)
        {
            return gridLevels[i].Unlocked;
        }

        public void UnlockLevel(Elevator selectedElevator, int selectedLevel)
        {
            gridLevels[selectedLevel].Unlocked = true;
        }

        public void SetupAttachments()
        {
            // TODO no need to do this I think, I'll replace all of it
            /*
            AddToGridChange((_, _) => SceneRefs.GridTiles.ChangeSelMode(ControlMode.Nothing));
            AddToGridChange((_, _) => SceneRefs.GridTiles.Exit());
            AddToGridChange(SceneRefs.Humans.SwitchLevel);
            ChangeGridLevel(0);*/
        }
/*
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gridPos"></param>
        /// <returns></returns>
        public bool DamageRock(int3 gridPos, int ammount)
        {

        }*/
    }
}
