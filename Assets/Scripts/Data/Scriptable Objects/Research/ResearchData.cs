using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>Represents one thing to research, can have prequiseites and folowing ones.</summary>
[Serializable]
public class ResearchNode
{
    #region Variables
    /// <summary>Name of the research.</summary>
    public string name;
    /// <summary>ID of the research.</summary>
    public int id;
    /// <summary>Level of the research</summary>
    public int level;

    /// <summary>Was it already researched?</summary>
    public bool researched;
    /// <summary>Current progress time.</summary>
    public float currentTime;
    /// <summary>Target progress time.</summary>
    public int researchTime;

    /// <summary>Cost to start research(WIP).</summary>
    public Resource reseachCost;

	/// <summary>If the button is unlocking a building.</summary>
	public bool buildingNode;
	/// <summary>Category of foreing elements that is assigned to.</summary>
	public int nodeCategory;
    /// <summary>Element id from the category.</summary>
    public int nodeAssignee;
    /// <summary>Prequisite needed nodes.</summary>
    [SerializeField] public List<int> unlockedBy;
    /// <summary>Next nodes.</summary>
    [SerializeField] public List<int> unlocks;

#if UNITY_EDITOR_WIN
    [SerializeField] public Color lineColor = Color.red;
#endif
#endregion

    #region Overrides
    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;
        try
        {
            ResearchNode node = (ResearchNode)obj;
            if (id == node?.id)
                return true;
        }
        catch
        {
            //Debug.Log("Somthing went wrong");
        }
        return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
    #endregion

    public ResearchNode()
    {

    }

	public ResearchNode(ResearchNode node)
	{
		id = node.id;
		level = node.level;
		name = node.name;
		currentTime = node.currentTime;
		researchTime = node.researchTime;
		nodeCategory = node.nodeCategory;
		nodeAssignee = node.nodeAssignee;
		researched = node.researched;
		unlockedBy = node.unlockedBy;
		unlocks = node.unlocks;
		reseachCost = new();
		if (node.reseachCost != null)
		{
			reseachCost.type = node.reseachCost.type.ToList();
			reseachCost.ammount = node.reseachCost.ammount.ToList();
		}
	}

	#region Editor modifications
#if UNITY_EDITOR

	/// <summary>
	/// Used when creating.
	/// </summary>
	/// <param name="_gp"></param>
	/// <param name="_data"></param>
	/// <param name="level"></param>
	public ResearchNode(int _level, int _id)
    {
        level = _level;
        name = $"node {_level}";

        id = _id;
        researchTime = 100 * (_level + 1);

        currentTime = 0;
        researched = false;
        nodeCategory = -1;
        nodeAssignee = -1;
        unlockedBy = new();
        unlocks = new();
        reseachCost = new();
    }


	/// <summary>
	/// Connects this node as a prequisete to <paramref name="node"/>.
	/// </summary>
	/// <param name="node">Connecting node.</param>
	public void ConnectNode(ResearchNode node)
    {
        if (!unlocks.Contains(node.id))
        {
            unlocks.Add(node.id);
            node.unlockedBy.Add(id);
        }
    }

    /// <summary>
    /// Disconnects all connected nodes.
    /// </summary>
    public void DisconnectNodes(List<ResearchNode> category, bool justTop = false)
    {
        
        if(justTop == false)
		{
			for (int i = unlocks.Count - 1; i >= 0; i--)
				DisconnectNode(true, i, category);
		}

		for (int i = unlockedBy.Count - 1; i >= 0; i--)
			DisconnectNode(false, i, category);
	}

    /// <summary>
    /// Disconnects one node.
    /// </summary>
    /// <param name="disconectUp">if true disconects node form unlocks</param>
    /// <param name="index">index in list</param>
    public void DisconnectNode(bool disconectUp, int index, List<ResearchNode> category)
    {
        if (disconectUp)
        {
            category.Find(q=> q.id == unlocks[index]).unlockedBy.Remove(id);
            unlocks.RemoveAt(index);
        }
        else
        {
            category.Find(q => q.id == unlockedBy[index]).unlocks.Remove(id);
            unlockedBy.RemoveAt(index);
        }
	}
#endif
#endregion
}

/// <summary>Research Category groups nodes into logical pages.</summary>
[Serializable]
public class ResearchCategory : DataCategory<ResearchNode>
{

#if UNITY_EDITOR_WIN
	public void AddNode(int level)
	{
		ResearchNode node = new(level, UniqueID());

		Objects.Add(node);
		Objects = Objects.OrderBy(q => q.level).ToList();
	}


	public override int UniqueID()
    { 
		int id;
		do
		{
			id = UnityEngine.Random.Range(0, int.MaxValue);
		}
		while (Objects.Count(q => q.id == id) > 0);
		return id;
    }
#endif

	public ResearchCategory()
    {

    }

    public ResearchCategory(string _name)
    {
        Name = _name;
        Objects = new();
    }
}

/// <summary>Contains all research data, that can be edited.</summary>
[CreateAssetMenu(fileName = "ResearchData", menuName = "UI Data/Research Holder", order = 2)]
public class ResearchData : DataHolder<ResearchCategory>
{
#if UNITY_EDITOR_WIN
	public override List<string> Choices() => Categories.Select(q => q.Name).ToList();
#endif
}
