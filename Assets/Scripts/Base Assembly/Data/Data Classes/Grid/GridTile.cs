
using System;
using System.Collections.Generic;
using System.Text;

public struct GridTile
{
    int stability;
    public int Stability => stability;

#nullable enable
    public Pipe Pipe { get; set; }
    public ClickableObject TileBase { get; set; }


    public void IncreaseStability(int toAdd) => stability += toAdd;
    public void DecreaseStability(int toRemove) => stability -= toRemove;

}
