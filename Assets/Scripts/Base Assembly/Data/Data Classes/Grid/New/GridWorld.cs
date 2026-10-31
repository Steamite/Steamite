using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World;
using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World.Buildings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Mathematics;
using Unity.Mathematics.Geometry;
using UnityEngine;
using UnityEngine.UIElements;

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
        public GridLevel CurrentLevel => currentLevel;
        LevelRenderer renderer;

        public WorldBuildings WorldBuildings;



        public int CenterScaled;
        [NonSerialized] public int LargestLevelDimensions;
        [NonSerialized] public int LargestLevelDimensionsScaled;

        [SerializeField] Mesh[] buildingMeshes;
        [SerializeField] Material buildingMaterial;
        Action<int, int> onLevelChange;
        public List<Chunk> chunks;

        [SerializeField] int2 inspectedChunk;

        Vector3[] offset = new Vector3[] { new(-0.5f,0,0), new(0,0, -0.5f), new(0.5f, 0, 0), new(0, 0, 0.5f) };

        private void OnDrawGizmos()
        {
            int dim = GridLevel.chunkDimensions;

            for (int i = 0; i < LargestLevelDimensions; i++)
            {
                for (int j = 0; j < LargestLevelDimensions; j++)
                {
                    Vector3 vec = new(i * dim, 0, j * dim);
                    Vector3 secVec = vec + new Vector3(0, 60, 0);
                    Gizmos.DrawLine(vec, secVec);
                }
            }


            if (currentLevel == null || inspectedChunk.x == 0 || inspectedChunk.y == 0)
                return;

            Vector3 startPosition = new(currentLevel.levelOffsetScaled + 0.5f, 1, currentLevel.levelOffsetScaled + 0.5f);

            int index = 0;
            for (int y = 0; y < currentLevel.levelDimensionsScaled; y++)
            {
                for (int x = 0; x < currentLevel.levelDimensionsScaled; x++)
                {
                    int a = currentLevel.path[index];
                    if (index == SceneRefs.MouseController.hoverObjectIndex)
                    {
                        Debug.Log($"{x}, {y}");
                    }
                    for (int i = 0; i < 4; i++)
                    {

                        if ((a & 0x1) == 1)
                            Gizmos.color = Color.white;
                        else
                            Gizmos.color = Color.black;
                        Gizmos.DrawLine(startPosition, startPosition + offset[i]);
                        a >>= 0x1;
                    }
                    index++;
                    startPosition.x++;
                }
                startPosition.x = currentLevel.levelOffsetScaled + 0.5f;
                startPosition.z++;
            }
        }
        public void LoadGrid(WorldSaveNew saveToLoad)
        {
            onLevelChange = null;
            currentLevelIndex = -1;

            LEVEL_COUNT = saveToLoad.gridSave.Length;
            
            gridLevels = new GridLevel[LEVEL_COUNT];

            GridLevel.chunkDimensions = saveToLoad.chunkDimensions;
            LargestLevelDimensions = saveToLoad.largestDimension;
            LargestLevelDimensionsScaled = LargestLevelDimensions * GridLevel.chunkDimensions;



            for (int i = 0; i < LEVEL_COUNT; i++)
            {
                gridLevels[i] = new GridLevel(saveToLoad.gridSave[i], i);
            }

            CenterScaled = (LargestLevelDimensions / 2) * GridLevel.chunkDimensions;

            WorldBuildings = new WorldBuildings();// )GetComponent<>();
            WorldBuildings.LoadState(saveToLoad.objectsSave.buildings);


            renderer = GetComponent<LevelRenderer>();
            renderer.AddRenderer(WorldBuildings.SetupRenderer(buildingMaterial, renderer));
            renderer.Init(LEVEL_COUNT, gridLevels, LargestLevelDimensions * saveToLoad.chunkDimensions);

            SetLevel(0);
        }

        public LevelGridTile GetTile(int x, int level, int y)
        {
            return gridLevels[level].renderGrid[gridLevels[level].levelDimensionsScaled * y + x];
        }

        public LevelGridTile GetActiveTile(int2 worldPosition)
        {
            return GetActiveTile(worldPosition.x, worldPosition.y);
        }

        public LevelGridTile GetActiveTile(int worldX, int worldY)
        {
            worldX -= currentLevel.levelOffsetScaled;
            worldY -= currentLevel.levelOffsetScaled;
            //Debug.Log($"{x}, {y}, {currentLevel.levelOffsetScaled}");
            
            int index = currentLevel.levelDimensionsScaled * worldY + worldX;
            if (index < 0 || index >= currentLevel.renderGrid.Length)
                return LevelGridTile.OutsideWall;
            return currentLevel.renderGrid[index];
        }

        public LevelGridTile GetActiveTile(int x, int y, out int index)
        {
            x -= currentLevel.levelOffsetScaled;
            y -= currentLevel.levelOffsetScaled;
            //Debug.Log($"{x}, {y}, {currentLevel.levelOffsetScaled}");

            index = currentLevel.levelDimensionsScaled * y + x;
            if (index < 0 || index >= currentLevel.renderGrid.Length)
                return LevelGridTile.OutsideWall;
            return currentLevel.renderGrid[index];
        }



        /// <summary>
        /// Old new
        /// </summary>
        /// <param name="attach"></param>
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
            WorldBuildings.Dispose();
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gridPos"></param>
        /// <returns></returns>
        public void DamageRock(int gridIndex, int ammount, int level = 0)
        {
            GridLevel gridLevel = gridLevels[level];
            //int pos = gridLevel.levelDimensionsScaled * gridPos.y + gridPos.x;
            LevelGridTile tile = gridLevel.renderGrid[gridIndex];
            tile.otherData -= ammount;
            
            if (tile.otherData <= 0)
            {
                tile = LevelGridTile.Road;
                gridLevel.SetTile(gridIndex, tile);
                gridLevel.RevealRocks(gridIndex);
                renderer.CleanPosAndUpdate(gridIndex);
                renderer.UpdateMesh(level, gridIndex, gridLevel, 2);
            }
            else
            {
                gridLevel.SetTile(gridIndex, tile);
                //gridLevel.HideRocks(pos);
            }
        }

        public void CreateWall(int pos, int level = 0)
        {
            GridLevel gridLevel = gridLevels[level];
            gridLevel.SetTile(pos, LevelGridTile.Wall(10, false));

            renderer.UpdateMesh(level, pos, gridLevel);
        }

        public int GetWorldIndex(int2 worldPosition) => worldPosition.y * LargestLevelDimensionsScaled + worldPosition.x;
    }
}
