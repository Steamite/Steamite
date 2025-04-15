using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class BuildingWrapper
{
	[SerializeField] public Building b;
	[SerializeField] public int id;
	[SerializeField] public Sprite preview;

	public BuildingWrapper(int _id)
	{
        id = _id;
	}
}
/// <summary>Helps serialize build categories.</summary>
[Serializable]
public class BuildCategWrapper : DataCategory<BuildingWrapper>
{
#if UNITY_EDITOR
	/// <summary>Hold editor data for showing columns.</summary>
	[SerializeField] public List<bool> columnStates;
	public List<BuildingWrapper> availableBuildings;
#endif

    public BuildCategWrapper() { }

    public BuildCategWrapper(string _name, Texture2D _categIcon)
    {
		Name = _name;
        Icon = _categIcon;
        Objects = new();
    }

    public override int UniqueID()
    {
		int i;
		do
		{
			i = UnityEngine.Random.Range(0, int.MaxValue);
		} while (Objects.Count(q => q.id == i) > 0);
		return i;
	}
}

///<summary>Holds all buildable building, creates builds buttons from this, and is linked to research.</summary>
[CreateAssetMenu(fileName = "BuildButtonCategory", menuName = "UI Data/BuildButton Holder", order = 1)]
public class BuildButtonHolder : DataHolder<BuildCategWrapper>
{
	public override List<string> Choices() => Categories.Select(q => q.Name).ToList();

	public bool ContainsBuilding(Building newValue)
	{
        if (newValue == null)
            return false;
        return Categories.SelectMany(q => q.Objects).Select(q => q.b).Contains(newValue);
	}
}