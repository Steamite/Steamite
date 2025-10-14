using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Orders
{
    
    [Serializable]
    public class ResourceGen
    {
        public ResourceType type;
        /// <summary>In percent.</summary>
        public int typeChance = 50;
        public Vector2 ammountRange = new(5, 20);
    }


    [CreateAssetMenu(fileName = "OrderGenConfig", menuName = "OrderGenConfig", order = 1)]
    public class OrderGenConfig : ScriptableObject
    {
        public Vector2Int timeToFail = new(5, 10);
        public Vector2Int trustLoss = new(40, 60);
        public Vector2Int trustGain = new(10, 15);

        public List<ResourceGen> resourceGens = new();

#if UNITY_EDITOR
        private void OnValidate()
        {
            bool dirty = false;
            try
            {
                List<string> strings = ResFluidTypes.GetResNamesList();

                while (resourceGens.Count < strings.Count)
                {
                    resourceGens.Add(new());
                    dirty = true;
                }
                while (resourceGens.Count > strings.Count)
                {
                    resourceGens.RemoveAt(resourceGens.Count - 1);
                    dirty = true;
                }
                for (int i = 0; i < resourceGens.Count - 1; i++)
                {
                    if (resourceGens[i].type != ResFluidTypes.GetResByIndex(i + 1))
                    {
                        resourceGens[i].type = ResFluidTypes.GetResByIndex(i + 1);
                        dirty = true;
                    }
                }
                if (dirty)
                    EditorUtility.SetDirty(this);
            }
            catch
            {

            }
        }
#endif
    }
}