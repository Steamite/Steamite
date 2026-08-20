using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Unity.Collections;
using UnityEngine;

public abstract class GridTileOverlay : BaseOverlay
{
    public override void CalculateOverlay(NativeArray<float> overlay)
    {
        int size = MyGrid.GridSize;
        var grid = MyGrid.GetGridTilesCurrentLevel();

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int index = (y * size) + x;
                overlay[index] = Evaluate(grid[x, y]);
            }
        }
    }

    public abstract float Evaluate(GridTile tile);
}
