using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StartingLocations", menuName = "ScriptableObjects/Starting Locations", order = 1)]
public class StartingLocations : ScriptableObject
{
    public List<StartingLocation> startingLocations = new();
}
