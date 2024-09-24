using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class ResearchNode
{
    public int id;
    public GridPos gp;
    public float realX;
    public string name;
    public int researchTime;
    public int buttonCategory;
    public int buildButton;
    public bool researched;
    [SerializeField] public List<int> unlockedBy;
    [SerializeField] public List<int> unlocks;

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
    /// <param name="lastID"></param>
    public ResearchNode(GridPos _gp, string _data, int level, int lastID)
    {
        gp = _gp;
        gp.level = level;
        name = _data;
        id = lastID+1;

        buttonCategory = -1;
        buildButton = -1;
        researched = false;
        unlockedBy = new();
        unlocks = new();
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
    public ResearchNode(GridPos _gp, string _data, int _categ, int _buildButton, List<int> _unlockedBy, List<int> _unlocks, int _id)
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
    }

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

    public List<string> GetUnassignedBuildings(ResearchNode node)
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
