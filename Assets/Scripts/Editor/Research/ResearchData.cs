using Codice.Client.Common.TreeGrouper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class ResearchNode
{
    public int id;
    public GridPos gp;
    public string name;
    public int researchTime;
    public int buttonCategory;
    public int buildButton;
    [SerializeField] public List<ResearchNode> unlockedBy;
    [SerializeField] public List<ResearchNode> unlocks;

    public override bool Equals(object obj)
    {
       ResearchNode node = (ResearchNode)obj;
        if (node == null || id != node.id)
            return false;
        return true;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
    /// <summary>
    /// Used when creating.
    /// </summary>
    /// <param name="_gp"></param>
    /// <param name="_data"></param>
    /// <param name="level"></param>
    public ResearchNode(GridPos _gp, string _data, int level, int lastID)
    {
        gp = _gp;
        gp.level = level;
        name = _data;
        buttonCategory = -1;
        buildButton = -1;
        unlockedBy = new();
        unlocks = new();
        id = lastID+1;
    }

    /// <summary>
    /// Used for cloning, need all parametrs.
    /// </summary>
    /// <param name="_gp"></param>
    /// <param name="_data"></param>
    /// <param name="_categ"></param>
    /// <param name="_buildButton"></param>
    /// <param name="_unlockedBy"></param>
    /// <param name="_unlocks"></param>
    public ResearchNode(GridPos _gp, string _data, int _categ, int _buildButton, List<ResearchNode> _unlockedBy, List<ResearchNode> _unlocks, int _id)
    {
        gp = _gp;
        name = _data;
        buttonCategory = _categ;
        buildButton = _buildButton;
        unlockedBy = _unlockedBy.ToList();
        unlocks = _unlocks.ToList();
        id = _id;
    }

    public ResearchNode Clone()
    {
        return new(gp, name, buttonCategory, buildButton, unlocks, unlockedBy, id);
    }

    public void ConnectNode(ResearchNode node)
    {
        if (!unlocks.Contains(node))
        {
            unlocks.Add(node);
            node.unlockedBy.Add(this);
        }
    }

    /// <summary>
    /// Disconnects all connected nodes.
    /// </summary>
    public void DisconnectNodes()
    {
        for (int i = unlocks.Count-1; i >= 0; i--)
        {
            DisconnectNode(true, i);
        }
        for (int i = unlockedBy.Count -1; i >= 0; i--)
        {
            DisconnectNode(false, i);
        }
    }

    /// <summary>
    /// Disconnects one node.
    /// </summary>
    /// <param name="disconectUp">if true disconects node form unlocks</param>
    /// <param name="index">index in list</param>
    public void DisconnectNode(bool disconectUp, int index)
    {
        if (disconectUp)
        {
            unlocks[index].unlockedBy.Remove(this);
            unlocks.RemoveAt(index);
        }
        else
        {
            unlockedBy[index].unlocks.Remove(this);
            unlockedBy.RemoveAt(index);
        }
    }
}

[Serializable]
public class ResearchCategory
{
    public string categName;
    public List<ResearchNode> nodes;

    public ResearchCategory(string _name)
    {
        categName = _name;
        nodes = new();
    }
}

[CreateAssetMenu(fileName = "ResearchData", menuName = "ScriptableObjects/Research Holder", order = 2)]
public class ResearchData : ScriptableObject
{
    public BuildButtonHolder buildButtons;
    public List<ResearchCategory> categories = new();
    Dictionary<int, List<string>> allBuildings;
    Dictionary<int, List<string>> unassignedBuildings;
    static bool init = false;

    void OnValidate()
    {
        Init();
        if (!init)
        {
            foreach (ResearchCategory categ in categories)
            {
                foreach (ResearchNode node in categ.nodes)
                {
                    for(int i = 0; i < node.unlocks.Count; i++)
                    {
                        node.unlocks[i] = categ.nodes.First(q => q.id == node.unlocks[i].id);
                    }
                    for(int i = 0; i < node.unlockedBy.Count; i++)
                    {
                        node.unlockedBy[i] = categ.nodes.First(q => q.id == node.unlockedBy[i].id);
                    }
                }
            }
            init = true;
        }
    }

    public void Init()
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

    public void SelectBuilding(ResearchNode node)
    {
        if (node.buttonCategory > -1 && node.buildButton > -1)
            unassignedBuildings[node.buttonCategory].Remove(allBuildings[node.buttonCategory][node.buildButton]);
    }

    public void DeselectBuilding(ResearchNode node)
    {
        if (node.buttonCategory > -1 && node.buildButton > -1)
        {
            string s = allBuildings[node.buttonCategory][node.buildButton];
            if (unassignedBuildings[node.buttonCategory].IndexOf(s) == -1)
                unassignedBuildings[node.buttonCategory].Add(s);
        }
    }

    string GetBuildName(ResearchNode node)
    {
        return allBuildings[node.buttonCategory][node.buildButton];
    }

    public List<string> GetUnssignedBuildings(ResearchNode node)
    {
        List<string> s = new();
        if (node.buildButton != -1)
            s.Add(GetBuildName(node));
        s.AddRange(unassignedBuildings[node.buttonCategory]);
        return s;
    }

    public int GetIndex(int categ, string v)
    {
        return allBuildings[categ].IndexOf(v);
    }
}
