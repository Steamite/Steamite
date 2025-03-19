using System.Collections.Generic;
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
        if ((SceneRefs.gridTiles.markedTiles.Count > 0 || SceneRefs.gridTiles.activeControl == ControlMode.build))
            base.OnPointerDown(eventData);
    }

    /// <summary>
    /// If building or digging, calls the base <see cref="ClickableObject.OnPointerUp(PointerEventData)"/>.
    /// </summary>
    /// <param name="eventData"><inheritdoc/></param>
    public override void OnPointerUp(PointerEventData eventData)
    {
        if (SceneRefs.gridTiles.markedTiles.Count > 0 || SceneRefs.gridTiles.activeControl == ControlMode.build)
        {
            base.OnPointerUp(eventData);
        }
        else if(eventData.button != PointerEventData.InputButton.Left)
        {
            SceneRefs.gridTiles.BreakAction();
        }
    }
    #endregion Mouse Events

    #region Save
    public override ClickableObjectSave Save(ClickableObjectSave clickable = null) => null;
    #endregion
}
