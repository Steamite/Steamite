using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[Serializable]
public class BuildingWrapper
{
    [SerializeField] Building b;
    public Building building => b;//{ get => b; private set => b = value; }
    [SerializeField] public int id;
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

    public BuildingWrapper(int _id)
    {
        id = _id;
    }
}
/// <summary>Helps serialize build categories.</summary>
[Serializable]
public class BuildCategWrapper : DataCategory<BuildingWrapper>
{
    /// <summary>Hold editor data for showing columns.</summary>
    [NonSerialized] public List<bool> columnStates;
    [NonSerialized] public List<BuildingWrapper> availableBuildings;
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
public class BuildingData : DataHolder<BuildCategWrapper>
{
    #region Editor
#if UNITY_EDITOR
    public override List<string> Choices() => Categories.Select(q => q.Name).ToList();

    public bool ContainsBuilding(Building newValue)
    {
        if (newValue == null)
            return false;
        return Categories.SelectMany(q => q.Objects).Select(q => q.building).Contains(newValue);
    }
    public override int UniqueID()
    {
        int i;
        do
        {
            i = UnityEngine.Random.Range(0, int.MaxValue);
        } while (Categories.SelectMany(q=> q.Objects).Count(q => q.id == i) > 0);
        return i;
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