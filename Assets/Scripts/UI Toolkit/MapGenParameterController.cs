using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace StartMenu
{

    /// <summary>Gathers data from all parameters and merges them into the seed.</summary>
    public class MapGenParameterController : MonoBehaviour, IToolkitController
    {
        /// <summary>Parent reference for all parameters.</summary>
        VisualElement mapParamsElem;
        TextField seedParam;
        /// <summary>Notify other values.</summary>
        bool changeSeed = true;

        #region generation props
        string seed = "";
        public string Seed => seed;
        List<int> parameterValues;
        
        /// <summary>List of all available parameter controls.</summary>
        List<DropdownField> parameters;
        #endregion


        [SerializeField]
        List<string> sizes;

        [SerializeField]
        List<string> states;

        void GenerateRandomSeed()
        {
            parameters = new();
            parameterValues = new();
            for (int i = 0; i < 4; i++)
            {
                parameterValues.Add(1);
                int r = (Random.Range(0, 5) * 3) + 1;
                if (r > 9)
                {
                    seed += (char)(r + 55);
                }
                else
                    seed += r;
            }

            // Random seed
            for (int i = 0; i < 4; i++)
            {
                int random = Random.Range(0, 16);
                if (random > 9)
                {
                    char c = (char)(random + 55);
                    seed += c;
                }
                else
                    seed += random;
            }
        }

        /// <summary>
        /// Adds paramaters to the list(<see cref="parameters"/>)
        /// </summary>
        /// <param name="_root">Element containing "Map-Parameters" element.</param>
        public void Init(VisualElement _root)
        {
            GenerateRandomSeed();

            mapParamsElem = _root.Q<VisualElement>("MapParams");
            mapParamsElem.Add(seedParam = new TextField("Seed") { value = seed });
            
            seedParam.AddToClassList("param");
            seedParam.pickingMode = PickingMode.Ignore;
            seedParam[0].pickingMode = PickingMode.Ignore;
            seedParam[1].pickingMode = PickingMode.Position;
            seedParam.RemoveFromClassList("unity-base-text-field");
            seedParam.RegisterValueChangedCallback((evt) =>
            {
                seed = evt.newValue;
                UpdateEnums();
            });


            // map size
            AddParam("Map Size", sizes);


            Label label;
            mapParamsElem.Add(label = new Label("Veins"));
            label.AddToClassList("param-title");

            /// VEINS ///
            AddParam("Size", sizes);
            AddParam("Richness", states);
            AddParam("Number", states);
        }

        void AddParam(string name, List<string> choices)
        {
            DropdownField field = new(name, choices, 1);
            field.pickingMode = PickingMode.Ignore;
            field[0].pickingMode = PickingMode.Ignore;
            field[1].pickingMode = PickingMode.Position;

            var i = parameters.Count;
            field.AddToClassList("param");
            parameters.Add(field);
            mapParamsElem.Add(field);
            field.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                UpdateSeed(i, field.index);
            });
                
        }

        /// <summary>
        /// When changing enum val, changes seed.
        /// </summary>
        /// <param name="index">enum index</param>
        /// <param name="value">new enum value</param>
        void UpdateSeed(int index, int value)
        {
            parameterValues[index] = value;
            if (changeSeed)
            {
                string _seed = "";
                for (int i = 0; i < seed.Length; i++)
                {
                    if (i == index)
                    {
                        int r = UnityEngine.Random.Range(0, 5) * 3 + value;
                        if (r > 9)
                        {
                            _seed += (char)(r + 55);
                        }
                        else
                            _seed += r;
                        continue;
                    }
                    _seed += seed[i];
                }

                seed = _seed;
                seedParam.SetValueWithoutNotify(_seed);
            }
        }

        /// <summary>
        /// When changing seed, updates all enums.
        /// </summary>
        /// <param name="s"></param>
        void UpdateEnums()
        {
            for (int i = 0; i < 4 && i < seed.Length; i++)
            {
                changeSeed = false;

                parameters[i].index = MyMath.HexToDec($"{seed[i]}") % 3;
                changeSeed = true;
            }
        }
    }
}