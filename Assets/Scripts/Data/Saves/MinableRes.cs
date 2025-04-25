using System;
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

    /// <summary>
    /// Randomly chosen value.
    /// </summary>
    /// <param name="genParam"></param>
    /// <returns></returns>
    public int Value(int genParam) => UnityEngine.Random.Range(min[genParam], max[genParam]);

    public VeinParameter()
    {
        min = new int[3];
        max = new int[3];
    }

}

/// <summary>Different types of <see cref="Rock"/> that can be spawned.</summary>
[CreateAssetMenu(fileName = "MinableRes", menuName = "MapGen/MinableRes", order = 1)]
public class MinableRes : ScriptableObject
{
    /// <summary>Resource type it contains.</summary>
    [SerializeField] public ResourceType resource;
    /// <summary>Base integrity of the tile.</summary>
    [SerializeField] public int hardness;
    /// <summary>Color of the tile.</summary>
    [SerializeField] public Color color;
    /// <summary>Posible richness.</summary>
    [SerializeField] public VeinParameter[] richness = new VeinParameter[5];
    /// <summary>Posible size.</summary>
    [SerializeField] public VeinParameter[] size = new VeinParameter[5];
    /// <summary>Posible count.</summary>
    [SerializeField] public VeinParameter[] count = new VeinParameter[5];

    public MinableRes()
    {
    }
}