using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[Serializable]
public class BuildingWrapper : DataObject
{
    public Building building => b;
    [SerializeField] Building b;

    public override string GetName() => b?.objectName;


    [SerializeField] public Sprite preview;
    [NonSerialized] public bool unlocked = true;


#if UNITY_EDITOR
    public void SetBuilding(Building _b, byte categoryID, string name = null)
    {
        b = _b;
        if (b)
        {
            if (name != null)
                b.objectName = name;
            b.categoryID = categoryID;
            b.wrapperID = id;
            EditorUtility.SetDirty(b);
        }
    }
#endif

    public BuildingWrapper(int _id) : base(_id)
    {
    }

    public BuildingWrapper(){}
}
/// <summary>Helps serialize build categories.</summary>
[Serializable]
public class BuildCategWrapper : DataCategory<BuildingWrapper>
{
    /// <summary>Hold editor data for showing columns.</summary>
    [NonSerialized] public List<bool> columnStates;
    public BuildCategWrapper() { }

    public BuildCategWrapper(string _name, Texture2D _categIcon)
    {
        Name = _name;
        Icon = _categIcon;
        Objects = new();
    }
}

///<summary>Holds all buildable building, creates builds buttons from this, and is linked to research.</summary>
[CreateAssetMenu(fileName = "BuildButtonCategory", menuName = "UI Data/BuildButton Holder", order = 1)]
public class BuildingData : DataHolder<BuildCategWrapper, BuildingWrapper>
{
    #region Editor
#if UNITY_EDITOR

    public bool ContainsBuilding(Building newValue)
    {
        if (newValue == null)
            return false;
        return Categories.SelectMany(q => q.Objects).Select(q => q.building).Contains(newValue);
    }

#endif
    #endregion

    public Building GetBuilding(int categ, int id)
    {
        return Categories[categ].Objects.Find(q => q.id == id).building;
    }

    public Building GetBuilding(string name)
    {
        return Categories[0].Objects.Find(q => q.building.objectName == name).building;
    }
}