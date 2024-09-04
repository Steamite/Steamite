using System.Collections.Generic;
using UnityEngine.EventSystems;

public class Road : ClickableObject
{
    public List<int> entryPoints = new(); // IDs of builds that have entry points on this piece of 
    public override void OnPointerUp(PointerEventData eventData)
    {
        if ((MyGrid.gridTiles.markedTiles.Count > 0 || MyGrid.gridTiles.selMode == SelectionMode.build) && eventData.button == PointerEventData.InputButton.Left)
        {
            MyGrid.gridTiles.Up();
        }
        else if(eventData.button != PointerEventData.InputButton.Left)
        {
            MyGrid.gridTiles.BreakAction();
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if ((MyGrid.gridTiles.markedTiles.Count > 0 || MyGrid.gridTiles.selMode == SelectionMode.build) && eventData.button == PointerEventData.InputButton.Left)
        {
            MyGrid.gridTiles.Down();
        }
        else if (eventData.button != PointerEventData.InputButton.Left)
        {
            MyGrid.gridTiles.BreakAction();
        }
    }
    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        return null;
    }
}
