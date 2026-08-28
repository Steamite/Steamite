using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Overlay : GridTilesMode
{
    public override void ExitMod()
    {
        SceneRefs.Overlays.overlay.ChangeOverlay(-1);
    }

    public override void DownObject()
    {
        var MousePos = SceneRefs.Overlays.overlay.MouseHitPoint;
        if (MousePos.x < 0 && MousePos.y < 0)
            return;

        var tile = MyGrid.GetGridTile(MousePos.x, MousePos.y).TileBase;
        if (tile is IStabilitySupport stability)
        {
            RadiusScanUtil util = new(MousePos, stability.SupportValue);
            SceneRefs.Overlays.overlay.SetSelectedTiles(util.DoRadius());
        }
        else if(tile is IEffectObject effect)
        {
            //RadiusScanUtil util = new(effect.EffectPos.ToVecInt(), effect.Range.currentValue);
            SceneRefs.Overlays.overlay.SetSelectedTiles(effect.EffectRoads.Select(q=> q.GetPos().ToVecInt()).ToList());
        }
        else
            SceneRefs.Overlays.overlay.ClearSelectedTiles();
    }
}