using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


[Serializable]
public class BuildCategWrapper
{
    [SerializeField] public string categName;
    [SerializeField] public List<Building> buildings;
}

[CreateAssetMenu(fileName = "BuildButtonCategory", menuName = "UI Data/BuildButton Holder", order = 1)]
public class BuildButtonHolder : ScriptableObject
{
    /// <summary>
    /// Takes filled prefabs and creates coresponding buttons.
    /// </summary>
    [SerializeField] public List<BuildCategWrapper> buildingCategories = new List<BuildCategWrapper>();
    public Action onChange;
}