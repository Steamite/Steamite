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
    /// <summary>Position of the research</summary>
    public GridPos gp;
    /// <summary>Scaled position.</summary>
    public float realX;

    /// <summary>Was it already researched?</summary>
    public bool researched;
    /// <summary>Current progress time.</summary>
    public float currentTime;
    /// <summary>Target progress time.</summary>
    public int researchTime;

    /// <summary>Cost to start research(WIP).</summary>
    public Resource reseachCost;

    /// <summary>Category the button is from.</summary>
    public int buttonCategory;
    /// <summary>Build button to unlock.</summary>
    public int buildButton;
    /// <summary>Prequisite needed nodes.</summary>
    [SerializeField] public List<int> unlockedBy;
    /// <summary>Next nodes.</summary>
    [SerializeField] public List<int> unlocks;
    #endregion

    #region Overrides
    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;
        try
        {
            ResearchNode node = (ResearchNode)obj;
            if (id == node.id)
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

    #region Constructors
    public ResearchNode()
    {

    }
    /// <summary>
    /// Used when creating.
    /// </summary>
    /// <param name="_gp"></param>
    /// <param name="_data"></param>
    /// <param name="level"></param>
    /// <param name="lastID"></param>
    public ResearchNode(GridPos _gp, string _data, int level, int lastID)
    {
        gp = _gp;
        gp.y = level;
        name = _data;
        id = lastID+1;
        researchTime = 100 * ((int)_gp.y + 1);

        currentTime = 0;
        researched = false;
        buttonCategory = -1;
        buildButton = -1;
        unlockedBy = new();
        unlocks = new();
        reseachCost = new();
    }

    public ResearchNode(ResearchNode node)
    {
        id = node.id;
        gp = node.gp;
        realX = node.realX;
        name = node.name;
        currentTime = node.currentTime;
        researchTime = node.researchTime;
        buttonCategory = node.buttonCategory;
        buildButton = node.buildButton;
        researched = node.researched;
        unlockedBy = node.unlockedBy;
        unlocks = node.unlocks;
        reseachCost = new();
        if(node.reseachCost != null)
        {
            reseachCost.type = node.reseachCost.type.ToList();
            reseachCost.ammount = node.reseachCost.ammount.ToList();
        }
    }
    #endregion

    #region Editor modifications
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
    public void DisconnectNodes(List<ResearchNode> category)
    {
        for (int i = unlocks.Count-1; i >= 0; i--)
        {
            DisconnectNode(true, i, category);
        }
        for (int i = unlockedBy.Count -1; i >= 0; i--)
        {
            DisconnectNode(false, i, category);
        }
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
    #endregion
}

/// <summary>Research Category groups nodes into logical pages.</summary>
[Serializable]
public class ResearchCategory
{
    /// <summary>Category name.</summary>
    [SerializeField] public string categName;
    /// <summary>List of all nodes in the group.</summary>
    [SerializeField] public List<ResearchNode> nodes;

    public ResearchCategory()
    {

    }

    public ResearchCategory(string _name)
    {
        categName = _name;
        nodes = new();
    }
}

/// <summary>Contains all research data, that can be edited.</summary>
[CreateAssetMenu(fileName = "ResearchData", menuName = "UI Data/Research Holder", order = 2)]
public class ResearchData : ScriptableObject
{
    #region Variables
    /// <summary>Reference to Build button data, for editing what research unlocks.</summary>
    public BuildButtonHolder buildButtons;
    /// <summary>All research categories.</summary>
    public List<ResearchCategory> categories = new();
    /// <summary>Filled when opening and changing data.</summary>
    [NonSerialized] Dictionary<int, List<string>> allBuildings;
    /// <summary>Filled when opening and changing data.</summary>
    [NonSerialized] Dictionary<int, List<string>> unassignedBuildings;
    #endregion

#if UNITY_EDITOR_WIN
    void OnValidate()
    {
        Init();
    }

    /// <summary>
    /// Fills <see cref="allBuildings"/> and <see cref="unassignedBuildings"/>.
    /// </summary>
    public void Init()
    {
        if (buildButtons)
        {
            allBuildings = new();
            unassignedBuildings = new();
            for (int i = 0; i < buildButtons.buildingCategories.Count; i++)
            {
                allBuildings.Add(i, new());
                unassignedBuildings.Add(i, new());
                foreach (Building building in buildButtons.buildingCategories[i].buildings)
                {
                    allBuildings[i].Add(building.name);
                    unassignedBuildings[i].Add(building.name);
                }
            }
            foreach (ResearchNode node in categories.SelectMany(q => q.nodes))
            {
                if (node.buttonCategory != -1 && node.buildButton != -1)
                    unassignedBuildings[node.buttonCategory].Remove(allBuildings[node.buttonCategory][node.buildButton]);
            }
            Debug.Log("Research init");
        }
        else
        {
            buildButtons = (BuildButtonHolder)Resources.Load("Holders/Data/BuildButtonData");
        }
    }

    /// <summary>
    /// Triggered by selecting a building to be unlocked by a research node.
    /// </summary>
    /// <param name="node">Modified node.</param>
    public void SelectBuilding(ResearchNode node)
    {
        if (node.buttonCategory > -1 && node.buildButton > -1)
            unassignedBuildings[node.buttonCategory].Remove(allBuildings[node.buttonCategory][node.buildButton]);
    }

    /// <summary>
    /// Triggered by unselecting a building from beeing unlocked by a research node.
    /// </summary>
    /// <param name="node">Modified node.</param>
    public void DeselectBuilding(ResearchNode node)
    {
        if (node.buttonCategory > -1 && node.buildButton > -1)
        {
            string s = allBuildings[node.buttonCategory][node.buildButton];
            if (unassignedBuildings[node.buttonCategory].IndexOf(s) == -1)
                unassignedBuildings[node.buttonCategory].Add(s);
        }
    }

    /// <summary>
    /// Finds name of the assigned building.
    /// </summary>
    /// <param name="node">Asking node</param>
    /// <returns>building name</returns>
    string GetBuildName(ResearchNode node)
    {
        try
        {
            return allBuildings[node.buttonCategory][node.buildButton];
        }
        catch
        {
            Debug.Log("fuck");
            return "";
        }
    }

    /// <summary>
    /// Finds which buildings can be assigned to <paramref name="node"/>.
    /// </summary>
    /// <param name="node">Asking node.</param>
    /// <returns></returns>
    public List<string> GetUnassignedBuildings(ResearchNode node)
    {
        List<string> s = new();
        if (node.buildButton != -1)
            s.Add(GetBuildName(node));
        s.AddRange(unassignedBuildings[node.buttonCategory]);
        return s;
    }

    /// <summary>
    /// Gets absolute building index in a category.
    /// </summary>
    /// <param name="categ">Which category to look in.</param>
    /// <param name="buildingName">Name of the building to look for.</param>
    /// <returns></returns>
    public int GetIndex(int categ, string buildingName)
    {
        return allBuildings[categ].IndexOf(buildingName);
    }
#endif
}
