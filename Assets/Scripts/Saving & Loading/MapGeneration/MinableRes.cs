using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains min and max for each ground level.<br/>
/// One struct for each map generation option.
/// </summary>
[Serializable]
public class VeinParameter
{
    [SerializeField][Range(0, 20)] public int[] min;
    [SerializeField][Range(0, 20)] public int[] max;

    public int Value(int genParam) => UnityEngine.Random.Range(min[genParam], max[genParam]);

    public VeinParameter() 
    {
        min = new int[3];
        max = new int[3];
    }

}


[CreateAssetMenu(fileName = "MinableRes", menuName = "MapGen/MinableRes", order = 1)]
public class MinableRes : ScriptableObject
{
    [SerializeField] public ResourceType resource;
    [SerializeField] public int hardness;
    [SerializeField] public VeinParameter[] richness = new VeinParameter[5];
    [SerializeField] public VeinParameter[] size = new VeinParameter[5];
    [SerializeField] public VeinParameter[] count = new VeinParameter[5];
    [SerializeField] public Color color;

    public MinableRes()
    {
    }
}