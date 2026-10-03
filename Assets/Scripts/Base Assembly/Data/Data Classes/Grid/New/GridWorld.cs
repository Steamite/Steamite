using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New
{
    [RequireComponent(typeof(LevelRenderer))]
    public class GridWorld : MonoBehaviour
    {
        public static int LEVEL_COUNT;
        [NonSerialized] public GridLevel[] gridLevels = null;

        LevelRenderer renderer;

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

        public void LoadGrid(WorldSaveNew saveToLoad)
        {
            LEVEL_COUNT = saveToLoad.gridSave.Length;
            gridLevels = new GridLevel[LEVEL_COUNT];

            GridLevel.chunkDimensions = saveToLoad.chunkDimensions;
            for (int i = 0; i < LEVEL_COUNT; i++)
            {
                gridLevels[i] = new GridLevel(saveToLoad.gridSave[i], i);
            }

            renderer = GetComponent<LevelRenderer>();
            renderer.Init(LEVEL_COUNT, gridLevels);

            GetComponent<MouseListener>().Init(this);
        }

    }
}
