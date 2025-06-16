using UnityEngine;

/// <summary>
/// Holds data about each Level, used mainly for display, and determening the cost of unlocking a new Level.
/// </summary>
[CreateAssetMenu(fileName = "Level Present", menuName = "UI Data/LevelPresent", order = 3)]
public class LevelPresent : ScriptableObject
{
    /// <summary>Name of all levels.</summary>
    public string[] headers = { "First floor", "Cave", "Water", "Random Header", "Depest point" };
    /// <summary>Descriptions of all levels.</summary>
    public string[] bodies = { "aaaaaaaaa", "bbbbbbb", "cccccc", "ddddddd", "eeeeeee" };
    /// <summary>Cost of all levels.</summary>
    public MoneyResource[] costs = new MoneyResource[5];
}