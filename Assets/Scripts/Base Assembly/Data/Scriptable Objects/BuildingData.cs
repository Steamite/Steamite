using Levels;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;



[Serializable]
public class BuildingWrapper : DataObject
{
    public Building Building => building;
    [FormerlySerializedAs("b")]
    [SerializeField] Building building;

    public List<BuildingModifications> modifications;

    public override string GetName() => building?.Name;

#if UNITY_EDITOR
    [SerializeField] public int selectedLevel;
#endif
    [SerializeField] public Sprite preview;
    [NonSerialized] public bool unlocked = true;
    [NonSerialized] public List<Material> materials = new();


#if UNITY_EDITOR
    public void SetBuilding(Building _b, byte categoryID, string name = null)
    {
        building = _b;
        if (building)
        {
            if (name != null)
                building.Name = name;
            building.PrefabConnection = new(categoryID, id);
            EditorUtility.SetDirty(building);
        }
    }
#endif

    public BuildingWrapper(int _id) : base(_id)
    {
    }

    public BuildingWrapper() { }
}

/// <summary>Helps serialize build categories.</summary>
[Serializable]
public class BuildCategWrapper : DataCategory<BuildingWrapper>
{
    /// <summary>Hold editor data for showing columns.</summary>
    [NonSerialized] public List<bool> columnStates;
    public BuildCategWrapper() { }

    public BuildCategWrapper(string _name, VectorImage _categIcon)
    {
        Name = _name;
        Icon = _categIcon;
        Objects = new();
    }
}

///<summary>Holds all buildable building, creates builds buttons from this, and is linked to research.</summary>
[CreateAssetMenu(fileName = "BuildButtonCategory", menuName = "UI Data/BuildButton Holder", order = 1)]
public class BuildingData : InitializableHolder<BuildCategWrapper, BuildingWrapper>
{
    public new const string PATH = "Build Data";
    #region Editor
#if UNITY_EDITOR
    public new const string EDITOR_PATH = "Assets/Game Data/Research and Building/Build Data.asset";

    public bool ContainsBuilding(Building newValue)
    {
        if (newValue == null)
            return false;
        return Categories.SelectMany(q => q.Objects).Select(q => q.Building).Contains(newValue);
    }

#endif
#endregion

    public Building GetBuilding(int categ, int id)
    {
        return Categories.FirstOrDefault(q => q.id == categ).Objects.Find(q => q.id == id).Building;
    }

    public Building GetBuilding(string name)
    {
        return Categories[0].Objects.Find(q => q.Building.Name == name).Building;
    }

    public Pipe GetPipe()
    {
        return Categories[3].Objects.Find(q => q.Building is Pipe).Building as Pipe;
    }

    public override void Init()
    {
        foreach (var category in Categories)
        {
            foreach (var obj in category.Objects)
            {
                obj.Building.InitPrefabData();
                obj.materials = obj.Building.GetComponentsInChildren<Renderer>().Select(q => q.sharedMaterial).ToList();
            }
        }
    }
}