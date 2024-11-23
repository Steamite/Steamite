using System;
using UnityEngine;

[CreateAssetMenu(fileName = "MinableRes", menuName = "MapGen/MinableRes", order = 1)]
public class MinableRes : ScriptableObject
{
    [SerializeField] public ResourceType resource;
    [SerializeField] public int hardness;
    [SerializeField] public int minAmmount;
    [SerializeField] public int maxAmmount;
    [SerializeField] public int minNodes;
    [SerializeField] public int maxNodes;
    [SerializeField] public int minGroups;
    [SerializeField] public int maxGroups;
    [SerializeField] public Color color;
    public MinableRes()
    {
        hardness = 2;
        minAmmount = 4;
        maxAmmount = 8;
        minNodes = 4;
        maxNodes = 10;
        minGroups = 5;
        maxGroups = 8;
    }
}