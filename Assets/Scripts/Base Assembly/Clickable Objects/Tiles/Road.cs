using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Provides paths for <see cref="Human"/>s.</summary>
public class Road : ClickableObject
{
    /// <summary>Contains ids of buildings that have entry points on this tile</summary>
    public List<int> entryPoints = new();

    #region Basic Operations
    /// <inheritdoc/>
    public override GridPos GetPos()
    {
        return new GridPos(
            transform.position.x,
            (transform.position.y - 0.45f) / 2,
            transform.position.z);
    }
    #endregion

    #region Mouse Events
    /// <inheritdoc/>
    public override void OnPointerDown(PointerEventData eventData)
    {
        if ((SceneRefs.GridTiles.tempMarkedTiles.Count > 0 || SceneRefs.GridTiles.activeControl == ControlMode.build))
            base.OnPointerDown(eventData);
    }

    /// <summary>
    /// If building or digging, calls the base <see cref="ClickableObject.OnPointerUp(PointerEventData)"/>.
    /// </summary>
    /// <param name="eventData"><inheritdoc/></param>
    public override void OnPointerUp(PointerEventData eventData)
    {
        if (SceneRefs.GridTiles.tempMarkedTiles.Count > 0 || SceneRefs.GridTiles.activeControl == ControlMode.build)
        {
            base.OnPointerUp(eventData);
        }
        else if (eventData.button != PointerEventData.InputButton.Left)
        {
            SceneRefs.GridTiles.BreakAction();
        }
    }
    #endregion Mouse Events

    #region Save
    public override ClickableObjectSave Save(ClickableObjectSave clickable = null) => null;

    public void RevealRocks()
    {
        GridPos pos = GetPos();
        int k;
        int baseX = Mathf.RoundToInt(pos.x);
        int baseZ = Mathf.RoundToInt(pos.z);
        GridPos checkPos = new(0, pos.y, 0);
        int radius = MyGrid.ROAD_SCAN_RADIUS;
        for (int i = -radius; i <= radius; i++)
        {
            for (int j = -radius; j <= radius; j++)
            {
                k = Math.Abs(i) + Math.Abs(j);
                if (k > 0 && k < MyGrid.ROAD_SCAN_RADIUS + 1)
                {
                    checkPos.x = baseX + i;
                    checkPos.z = baseZ + j;
                    if (MyGrid.GetGridItem(checkPos) is Rock rock)
                    {
                        rock.Unhide();
                    }
                }
            }
        }
    }
}
    #endregion
