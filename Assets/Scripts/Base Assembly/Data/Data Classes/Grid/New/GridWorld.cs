using System;
using System.Collections.Generic;
using System.Text;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New
{
    public class GridWorld : MonoBehaviour
    {
        public static int LEVEL_COUNT;
        [NonSerialized] public GameLevel[] worldLevels = null;

        public void Init(WorldSaveNew saveToLoad)
        {
            LEVEL_COUNT = saveToLoad.gridSave.Length;
            worldLevels = new GameLevel[LEVEL_COUNT];

            GameLevel.chunkDimensions = saveToLoad.chunkDimensions;
            for (int i = 0; i < LEVEL_COUNT; i++)
            {
                worldLevels[i] = new GameLevel(saveToLoad.gridSave[i], i);
            }
        }
    }
}
