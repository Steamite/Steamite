using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World.Buildings;
using Assets.Scripts.Systems.Buildings;
using Levels;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;



[Serializable]
public class ResourceNew
{
    public DataAssign assign;

    public byte amount;
}

[Serializable]
public class MoneyNew
{
    public List<ResourceNew> resource;
    public short moneyCost;
}

[Serializable]
public class BuildingWrapper : DataObject
{
    public BuildingNew Building => building;
    [SerializeField] BuildingNew building;

    public List<BuildingModifications> modifications;

    public MoneyNew cost;

#if UNITY_EDITOR
    public int selectedLevel;
#endif
    public Sprite preview;
    
    public BuildingGrid blueprint;

    [NonSerialized] public bool unlocked = true;

    public BuildingWrapper(int _id) : base(_id)
    {
    }

    public BuildingWrapper() { }

    public void SetBuilding(BuildingNew building)
    {
        throw new NotImplementedException();
    }

    public ICollection<BuildingInventory> FillCost()
    {
        var resource = cost.resource;
        BuildingInventory[] buildingInventories = new BuildingInventory[resource.Count];

        for (int i = 0; i < resource.Count; i++)
        {
            ResourceNew item = resource[i];

            buildingInventories[i] = new()
            {
                type = ResFluidTypes.GetRuntimeIndex(item.assign),
                cost = item.amount,
                stored = 0
            };
        }

        return buildingInventories;
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
    [NonSerialized] List<BuildingWrapper> flatWrappers;
    public List<BuildingWrapper> FlatWrappers 
    { 
        get 
        {
            if (flatWrappers == null || flatWrappers.Count == 0)
                Init();
            return flatWrappers;
        }
    }
    public Mesh defaultMesh;
    public new const string PATH = "Build Data";
    #region Editor
#if UNITY_EDITOR
    public new const string EDITOR_PATH = "Assets/Game Data/Research and Building/Build Data.asset";
#endif
#endregion
    
    public override void Init()
    {
        for (int i = 0; i < Categories.Count; i++)
        {
            for (int j = 0; j < Categories[i].Objects.Count; j++)
            {
                if(Categories[i].Objects[j].blueprint.mesh == null)
                    Categories[i].Objects[j].blueprint.mesh = defaultMesh;
            }
        }


        flatWrappers = new();
        foreach (var categ in Categories)
        {
            foreach (var wrapper in categ.Objects)
            {
                flatWrappers.Add(wrapper);
            }
        }
    }

    public BuildingWrapper GetBuilding(byte categoryIndex, byte wrapperIndex)
    {
        return Categories[categoryIndex].Objects[wrapperIndex];
    }

    public BuildingWrapper GetBuilding(short buildingIndex)
    {
        return FlatWrappers[buildingIndex];// .Objects[wrapperIndex];
    }

    public short GetFlatId(int selectedChoice, int objectIndex)
    {
        int offset = 0;
        for (int i = 0; i < selectedChoice; i++)
        {
            offset += Categories[i].Objects.Count;
        }
        return (short)(offset + objectIndex);
    }
}