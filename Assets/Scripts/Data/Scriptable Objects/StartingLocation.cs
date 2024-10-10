using System;
using System.Collections.Generic;

[Serializable]
public class StartingLocation
{
    public string name;

    public int foodProduction;
    public int maxFoodProduction;

    public StartingLocation(string _name, int _foodProduction, int _maxFoodProduction)
    {
        name = _name;
        foodProduction = _foodProduction;
        maxFoodProduction = _maxFoodProduction;
    }
}