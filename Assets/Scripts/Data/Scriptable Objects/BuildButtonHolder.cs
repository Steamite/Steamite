using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class BuildingWrapper
{
	[SerializeField] public Building b;
	[SerializeField] public int id;

	public BuildingWrapper(int _id)
	{
        id = _id;
	}
}
/// <summary>Helps serialize build categories.</summary>
[Serializable]
public class BuildCategWrapper
{
#if UNITY_EDITOR
	/// <summary>Hold editor data for showing columns.</summary>
	[SerializeField] public List<bool> columnStates;
#endif
	/// <summary>Category name.</summary>
	[SerializeField] public string categName;
    /// <summary>Category icon for the category button.</summary>
    [SerializeField] public Texture2D categIcon;
    /// <summary>Buildings that belong in the category.</summary>
    [SerializeField] public List<BuildingWrapper> buildings;
    public BuildCategWrapper() { }

    public BuildCategWrapper(string _name, Texture2D _categIcon)
    {
        categName = _name;
        categIcon = _categIcon;
        buildings = new();
    }

    public int UniqueID 
    {
        get
        {
            int i;
            do
            {
                i = UnityEngine.Random.Range(0, int.MaxValue);
            } while (buildings.Count(q => q.id == i) > 0);
            return i;
        }
    }
}

///<summary>Holds all buildable building, creates builds buttons from this, and is linked to research.</summary>
[CreateAssetMenu(fileName = "BuildButtonCategory", menuName = "UI Data/BuildButton Holder", order = 1)]
public class BuildButtonHolder : ScriptableObject
{
    /// <summary>Takes filled prefabs and creates coresponding buttons.</summary>
    [SerializeField] public List<BuildCategWrapper> buildingCategories = new List<BuildCategWrapper>();

	public bool ContainsBuilding(Building newValue)
	{
        if (newValue == null)
            return false;
        return buildingCategories.SelectMany(q => q.buildings).Select(q => q.b).Contains(newValue);
	}
}