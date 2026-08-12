using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class StabilityOverlay : GridTileOverlay
{
    [SerializeField] float maxIntegrity;
    public float MaxIntegrity => maxIntegrity;


    protected override float Evaluate(GridTile[,] grid, int x, int y)
    {
        return grid[x, y].Stability / maxIntegrity;
    }
}

