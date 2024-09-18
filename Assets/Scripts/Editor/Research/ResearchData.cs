using Codice.Client.Common.TreeGrouper;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class ResearchNode
{
    public GridPos gp;
    public string data;
    public int buttonCategory;
    public int buildButton;

    public ResearchNode(GridPos _gp, string _data)
    {
        gp = _gp;
        data = _data;
        buttonCategory = -1;
        buildButton = -1;
    }

    public ResearchNode(GridPos _gp, string _data, int _categ, int _buildButton)
    {
        gp = _gp;
        data = _data;
        buttonCategory = _categ;
        buildButton = _buildButton;
    }

    public ResearchNode Clone()
    {
        return new(gp, data, buttonCategory, buildButton);
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
    
    public void Init()
    {
        OnValidate();
    }
    void OnValidate()
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
        foreach (ResearchNode node in categories.SelectMany(q=>q.nodes))
        {
            if(node.buttonCategory != -1 && node.buildButton != -1)
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
