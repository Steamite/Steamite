using System;
using UnityEngine;

[Serializable]
public class Build
{
    [Header("Size")]
    public BuildingGrid blueprint;
    public int heigth = 0;
    [Header("Storage")]
    public Resource cost;
    [Header("States")]
    public bool constructed = false;
    public bool deconstructing = false;
    public int constructionProgress = 0;
    public int maximalProgress = 0;
    public Build()
    {
    }
}
