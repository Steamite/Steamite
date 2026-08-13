using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class StabilityOverlay : GridTileOverlay
{
    [SerializeField] float maxIntegrity;
    public float MaxIntegrity => maxIntegrity;


    public override float Evaluate(GridTile tile)
    {
        return tile.Stability / maxIntegrity;
    }
}

