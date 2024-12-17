using System.Collections.Generic;
using UnityEngine.EventSystems;

public class Road : ClickableObject
{
    public List<int> entryPoints = new(); // IDs of builds that have entry points on this piece of 

    #region Basic Operations
    public override GridPos GetPos()
    {
        return new GridPos(
            transform.position.x,
            (transform.position.y - 0.45f) / 2,
            transform.position.z);
    }
    #endregion

    #region Mouse Events
    public override void OnPointerDown(PointerEventData eventData)
    {
        if ((SceneRefs.gridTiles.markedTiles.Count > 0 || SceneRefs.gridTiles.selMode == SelectionMode.build) && eventData.button == PointerEventData.InputButton.Left)
        {
            SceneRefs.gridTiles.Down();
        }
        else if (eventData.button != PointerEventData.InputButton.Left)
        {
            SceneRefs.gridTiles.BreakAction();
        }
    }
    public override void OnPointerUp(PointerEventData eventData)
    {
        if ((SceneRefs.gridTiles.markedTiles.Count > 0 || SceneRefs.gridTiles.selMode == SelectionMode.build) && eventData.button == PointerEventData.InputButton.Left)
        {
            SceneRefs.gridTiles.Up();
        }
        else if(eventData.button != PointerEventData.InputButton.Left)
        {
            SceneRefs.gridTiles.BreakAction();
        }
    }
    #endregion Mouse Events
    
    #region Save
    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        return null;
    }
    #endregion
}
