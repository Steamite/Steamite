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

[CreateAssetMenu(fileName = "BuildButtonCategory", menuName = "ScriptableObjects/BuildButton Holder", order = 1)]
public class BuildButtonHolder : ScriptableObject
{
    /// <summary>
    /// Takes filled prefabs and creates coresponding buttons.
    /// </summary>
    [SerializeField] public List<BuildCategWrapper> buildingCategories = new List<BuildCategWrapper>();
    public Action onChange;
}

[CustomEditor(typeof(BuildButtonHolder))]
public class BuildButtonHolderEditor : Editor
{
    List<BuildCategWrapper> categories;
    BuildButtonHolder holder;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (holder == null)
            holder = (BuildButtonHolder)target;
        if (categories == null)
            categories = holder.buildingCategories.ToList();
    }
}