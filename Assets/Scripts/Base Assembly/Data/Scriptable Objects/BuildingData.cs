using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World.Buildings;
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
    public BuildingNew Building => building;
    [SerializeField] BuildingNew building;

    public List<BuildingModifications> modifications;
    public MoneyResource Cost;
#if UNITY_EDITOR
    [SerializeField] public int selectedLevel;
#endif
    [SerializeField] public Sprite preview;
    [SerializeField] public Mesh mesh;
    
    public BuildingGrid blueprint;

    [NonSerialized] public bool unlocked = true;



#if UNITY_EDITOR
    /*public void SetBuilding( _b, int categoryID, string name = null)
    {
        building = _b;
        if (building)
        {
            if (name != null)
                building.Name = name;
            building.PrefabConnection = new(categoryID, id);
            EditorUtility.SetDirty(building);
        }
    }*/
#endif

    public BuildingWrapper(int _id) : base(_id)
    {
    }

    public BuildingWrapper() { }

    public void SetBuilding(BuildingNew building)
    {
        throw new NotImplementedException();
    }
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
#endif
#endregion

    /*public BuildingNew GetBuilding(int categ, int id)
    {
        //return Categories.FirstOrDefault(q => q.id == categ).Objects.Find(q => q.id == id).Building;
    }*/

    public BuildingNew GetBuilding(string name)
    {
        return Categories[0].Objects.Find(q => q.Name == name).Building;
    }
/*
    public Pipe GetPipe()
    {
        return Categories[3].Objects.Find(q => q.Building is Pipe).Building as Pipe;
    }*/

    public override void Init()
    {
        /*foreach (var category in Categories)
        {
            foreach (var obj in category.Objects)
            {
                //obj.Building.InitPrefabData();
                //obj.materials = obj.mesh.GetComponentsInChildren<Renderer>().Select(q => q.sharedMaterial).ToList();
            }
        }*/
    }
}