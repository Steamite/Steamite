using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>Helps serialize build categories.</summary>
[Serializable]
public class BuildCategWrapper
{
    /// <summary>Category name.</summary>
    [SerializeField] public string categName;
    /// <summary>Category icon for the category button.</summary>
    [SerializeField] public Sprite categIcon;
    /// <summary>Buildings that belong in the category.</summary>
    [SerializeField] public List<Building> buildings;
}

///<summary>Holds all buildable building, creates builds buttons from this, and is linked to research.</summary>
[CreateAssetMenu(fileName = "BuildButtonCategory", menuName = "UI Data/BuildButton Holder", order = 1)]
public class BuildButtonHolder : ScriptableObject
{
    /// <summary>Takes filled prefabs and creates coresponding buttons.</summary>
    [SerializeField] public List<BuildCategWrapper> buildingCategories = new List<BuildCategWrapper>();
}