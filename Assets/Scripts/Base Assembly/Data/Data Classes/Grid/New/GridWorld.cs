using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.Paths;
using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World;
using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World.Buildings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Collections;
using Unity.Entities;
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
        int currentLevelIndex;
        public int CurrentLevelIndex => currentLevelIndex;
        LevelRenderer renderer;

        public WorldBuildings WorldBuildings;

        NativeArray<int> offsetPathTableNeighbour;

        public int CenterScaled;
        [NonSerialized] public int LargestLevelDimension;
        [NonSerialized] public int LargestLevelDimensionScaled;
        [NonSerialized] public int GridSizeScaled;

        [SerializeField] Mesh[] buildingMeshes;
        [SerializeField] Material buildingMaterial;
        Action<int, int> onLevelChange;
        Action<LevelComponent, int, int> onLevelComponentChange;
        Action<LevelComponent, LevelComponent, int> LevelChange;
        public List<Chunk> chunks;

        [SerializeField] int2 inspectedChunk;

        /// <summary>
        /// 0 - Left <br/>
        /// 1 - Backwards <br/>
        /// 2 - Right <br/>
        /// 3 - Forward <br/>
        /// 4 - Down <br/>
        /// 5 - Up <br/>
        /// </summary>
        [NonSerialized] public NativeArray<short> path;
        [NonSerialized] public NativeArray<byte> pathRegions;
        [NonSerialized] public NativeBitArray regionList;

        readonly Vector3[] offset = new Vector3[] { new(-0.5f,0,0), new(0,0, -0.5f), new(0.5f, 0, 0), new(0, 0, 0.5f) };
        readonly Vector3[] verticalOffset = new Vector3[] { new(0, -0.5f,0), new(0, 0.5f, 0) };

        private void OnDrawGizmos()
        {
            #region Dim chunks corners
            int dim = LevelComponent.CHUNK_DIMENSIONS;

            // Draw chunk lines
            for (int i = 0; i < LargestLevelDimension; i++)
            {
                for (int j = 0; j < LargestLevelDimension; j++)
                {
                    Vector3 vec = new(i * dim, 0, j * dim);
                    Vector3 secVec = vec + new Vector3(0, 60, 0);
                    Gizmos.DrawLine(vec, secVec);
                }
            }
            #endregion

            if (currentLevelIndex == -1 || inspectedChunk.x == 0 || inspectedChunk.y == 0)
                return;

            Vector3 startPosition = new(0.5f, 1, 0.5f);

            int index = currentLevelIndex * GridSizeScaled;
            for (int y = 0; y < LargestLevelDimensionScaled; y++)
            {
                for (int x = 0; x < LargestLevelDimensionScaled; x++)
                {
                    int a = path[index];
                    for (int i = 0; i < 4; i++)
                    {
                        if ((a & 0x1) == 1)
                            Gizmos.color = Color.white;
                        else
                            Gizmos.color = Color.black;
                        Gizmos.DrawLine(startPosition, startPosition + offset[i]);
                        a >>= 0x1;
                    }

                    for (int i = 0; i < 2; i++)
                    {
                        if ((a & 0x1) == 1)
                        {
                            Gizmos.color = Color.white;
                            Gizmos.DrawLine(startPosition, startPosition + verticalOffset[i]);
                        }
                        a >>= 0x1;
                    }
                    index++;
                    startPosition.x++;
                }
                startPosition.x = 0.5f;
                startPosition.z++;
            }
        }

        WorldComponent world;
        [NonSerialized] public NativeArray<LevelComponent> levelArray;
        [NonSerialized] public NativeArray<OverlayComponent> overlayArray;

        [NonSerialized] public LevelComponent currentLevel;
        [NonSerialized] public OverlayComponent currentOverlay;

        public void LoadGrid(WorldSaveNew saveToLoad)
        {
            world = saveToLoad.world;
            levelArray = saveToLoad.levels;
            overlayArray = saveToLoad.overlays;

            onLevelChange = null;
            currentLevelIndex = -1;

            LEVEL_COUNT = levelArray.Length;

            // remove this:
            LargestLevelDimensionScaled = saveToLoad.world.LargestLevelDimensionScaled;
            LargestLevelDimension = saveToLoad.world.LargestLevelDimensionScaled / LevelComponent.CHUNK_DIMENSIONS;
            GridSizeScaled = LargestLevelDimensionScaled * LargestLevelDimensionScaled;

            offsetPathTableNeighbour = saveToLoad.world.offsets;

            path = saveToLoad.world.path;/*
            regionList = saveToLoad.world.regionList;
            pathRegions = saveToLoad.pathRegions;*/

            for (int i = 0; i < LEVEL_COUNT; i++)
            {
                //gridLevels[i] = new GridLevel(saveToLoad.levels[i], saveToLoad.overlays[i], i, LargestLevelDimensionScaled);
                var level = levelArray[i];
                var render = levelArray[i].renderGrid;
                for (int j = 0; j < render.Length; j++)
                {
                    if (render[j].TType == TileType.Road)
                        RevealChanges.RevealRocks(j, ref level);
                }
            }

            CenterScaled = (LargestLevelDimension / 2) * LevelComponent.CHUNK_DIMENSIONS;

            WorldBuildings = new WorldBuildings();
            WorldBuildings.LoadState(saveToLoad.objectsSave.buildings);

            GetComponent<IWorldInit>().Init(ref world, ref levelArray);
            GetComponent<IRenderInit>().Init(this);

            renderer = GetComponent<LevelRenderer>();
            renderer.AddRenderer(WorldBuildings.SetupRenderer(
                buildingMaterial, 
                renderer, 
                ref saveToLoad.world, 
                in saveToLoad.levels,
                ref saveToLoad.overlays));

            renderer.Init(this);

            SetLevel(0);
        }

        public LevelGridTile GetActiveTile(int2 worldPosition)
        {
            return GetActiveTile(worldPosition.x, worldPosition.y);
        }

        public LevelGridTile GetActiveTile(int worldX, int worldY)
        {
            worldX -= currentLevel.offsetsScaled;
            worldY -= currentLevel.offsetsScaled;
            //Debug.Log($"{x}, {y}, {currentLevel.levelOffsetScaled}");
            
            int index = currentLevel.dimensionsScaled * worldY + worldX;
            if (index < 0 || index >= currentLevel.renderGrid.Length)
                return LevelGridTile.OutsideWall;
            return currentLevel.renderGrid[index];
        }

        public LevelGridTile GetActiveTile(int x, int y, out int index)
        {
            x -= currentLevel.offsetsScaled;
            y -= currentLevel.offsetsScaled;
            //Debug.Log($"{x}, {y}, {currentLevel.levelOffsetScaled}");

            index = currentLevel.dimensionsScaled * y + x;
            if (index < 0 || index >= currentLevel.renderGrid.Length)
                return LevelGridTile.OutsideWall;
            return currentLevel.renderGrid[index];
        }



        /// <summary>
        /// Old new
        /// </summary>
        /// <param name="attach"></param>
        public void AttachChange(Action<int, int> attach) => onLevelChange += attach;
        public void AttachChange(Action<LevelComponent, int, int> attach) => onLevelComponentChange += attach;
        public void AttachChange(Action<LevelComponent, LevelComponent, int> attach) => LevelChange += attach;

        public void SetLevel(int i)
        {
            if (i == currentLevelIndex)
                return;

            int x = currentLevelIndex;
            currentLevelIndex = i;
            currentLevel = levelArray[i];
            currentOverlay = overlayArray[i];

            onLevelChange?.Invoke(x, i);
            onLevelComponentChange?.Invoke(currentLevel, x, i);
            LevelChange?.Invoke(levelArray[x], currentLevel, i);
        }

        private void OnDestroy()
        {
            WorldBuildings.Dispose();


            if(path.IsCreated)
                path.Dispose();
            if(pathRegions.IsCreated)
                pathRegions.Dispose();
            if (regionList.IsCreated)
                regionList.Dispose();

            if(offsetPathTableNeighbour.IsCreated)
                offsetPathTableNeighbour.Dispose();

            if (levelArray.IsCreated)
            {
                foreach (var level in levelArray)
                {
                    level.Dispose();
                }
                levelArray.Dispose();
            }

        }

        public bool IsUnlocked(int i)
        {
            return levelArray[i].Unlocked;
        }

        public void UnlockLevel(Elevator selectedElevator, int selectedLevel)
        {
            // TODO: need to use a pointer to change it
            //levelArray[selectedLevel] ; .Unlocked = true;
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
        public void DamageRock(int localIndex, uint ammount, int levelIndex)
        {
            LevelComponent level = levelArray[levelIndex];
            LevelGridTile tile = level.renderGrid[localIndex];
            tile.otherData -= ammount;
            
            if (tile.otherData <= 0)
            {
                WorldChanges.SetTile(localIndex, levelIndex, LevelGridTile.Road, ref world, in levelArray);
                RevealChanges.RevealRocks(localIndex, ref level);

                OverlayChanges.CleanPosAndUpdate(localIndex, levelArray[levelIndex], overlayArray[levelIndex]);
                renderer.UpdateOverlayTexture();
            }
            else
            {
                WorldChanges.SetTile(localIndex, levelIndex, tile, ref world, in levelArray);
            }
        }

        public void CreateWall(int pos, int level = 0)
        {
            WorldChanges.SetTile(pos, level, LevelGridTile.Wall(10, false), ref world, in levelArray);
        }

        public int GetWorldIndex(int2 worldPosition) => worldPosition.y * LargestLevelDimensionScaled + worldPosition.x;
    }
}
