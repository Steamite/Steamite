using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

[Serializable]
public class ResearchNode
{
    public Rect rect;
    public string data;
    public ResearchNode(Rect _r, string _data)
    {
        rect = _r;
        data = _data;
    }

    public ResearchNode Clone()
    {
        return new(rect, data);
    }
}

[Serializable]
public class ResearchCategory
{
    public string categName;
    public List<ResearchNode> heads;

    public ResearchCategory(string _name)
    {
        categName = _name;
        heads = new();
    }
}

[CreateAssetMenu(fileName = "ResearchData", menuName = "ScriptableObjects/Research Holder", order = 2)]
public class ResearchData : ScriptableObject
{
    public BuildButtonHolder buildButtons;
    public List<ResearchCategory> categories = new();
}
