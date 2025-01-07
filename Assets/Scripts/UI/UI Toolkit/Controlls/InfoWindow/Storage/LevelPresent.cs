using UnityEngine;

[CreateAssetMenu(fileName = "Level Present", menuName = "UI Data/LevelPresent", order = 3)]
public class LevelPresent : ScriptableObject
{
    public string[] headers = { "First floor", "Cave", "Water", "Random Header", "Depest point" };
    public string[] bodies = { "aaaaaaaaa", "bbbbbbb", "cccccc", "ddddddd", "eeeeeee" };
    public Resource[] costs = new Resource[5];
}