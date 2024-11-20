using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody))]
public class ClickableObject : MonoBehaviour, 
    IPointerEnterHandler, IPointerExitHandler, 
    IPointerDownHandler, IPointerUpHandler
{
    public bool selected = false;
    public int id = -1;

    ///////////////////////////////////////////////////
    ///////////////////Overrides///////////////////////
    ///////////////////////////////////////////////////
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;
        else if (((ClickableObject)obj).id == id && ((ClickableObject)obj).name == name)
            return true;
        return false;
    }

    public override int GetHashCode() { return base.GetHashCode(); }

    ///////////////////////////////////////////////////
    ///////////////////Methods/////////////////////////
    ///////////////////////////////////////////////////
    #region Basic Operations
    public virtual void UniqueID()
    {
        id = -1;
    }
    public virtual void GetID(JobSave jobSave)
    {
        jobSave.objectId = id;
        jobSave.objectType = typeof(ClickableObject);
    }

    public virtual string PrintText()
    {
        return "_";
    }
    #endregion Basic Operations
    #region Mouse Events
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        SceneRefs.gridTiles.Enter(this);
        if (SceneRefs.gridTiles.drag)
            eventData.pointerPress = gameObject;
    }
    public virtual void OnPointerExit(PointerEventData eventData)
    {
        SceneRefs.gridTiles.Exit(this);
        eventData.pointerPress = null;
    }
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (SceneRefs.gridTiles.drag == false && eventData.button == PointerEventData.InputButton.Left)
            SceneRefs.gridTiles.Down();
    }
    public virtual void OnPointerUp(PointerEventData eventData)
    {
        print(gameObject.name + $", {transform.position.x}, {transform.position.z}");
        if (eventData.button == PointerEventData.InputButton.Left)
            SceneRefs.gridTiles.Up();
        else
            SceneRefs.gridTiles.BreakAction();
    }
    #endregion Mouse Events
    #region Window
    /// <summary>
    /// Creates the info window, if first is false only updates the info.
    /// </summary>
    /// <param name="first">Prepare the window.</param>
    /// <returns>info window reference</returns>
    public virtual InfoWindow OpenWindow(bool setUp = false)
    {
        if (!selected)
            return null;
        InfoWindow info = CanvasManager.infoWindow;
        info.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new(0, 0);
        info.gameObject.SetActive(true);
        return info;
    }
    #endregion Window
    #region Saving
    public virtual ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        if (clickable == null)
            clickable = new();
        clickable.id = id;
        return clickable;
    }
    public virtual void Load(ClickableObjectSave save)
    {
        id = save.id;
    }
    #endregion Saving

    public virtual GridPos GetPos()
    {
        Debug.LogError("No Implementation");
        GridPos gp = new();
        return gp;
    }

    protected void CreateNewId(List<int> clickables) // creates a random int
    {
        do
        {
            id = Random.Range(0, int.MaxValue);
        } while (clickables.Where(q => q == id).Count() > 0);
        return;
    }
}
