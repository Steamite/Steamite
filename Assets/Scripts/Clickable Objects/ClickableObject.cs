using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickableObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public bool selected = false;
    public int id = -1;

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;
        else if (((ClickableObject)obj).id == id)
            return true;
        return false;
    }
    public override int GetHashCode() { return base.GetHashCode(); }

    public virtual void UniqueID()
    {
        id = -1;
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        MyGrid.gridTiles.Enter(this);
        if (MyGrid.gridTiles.drag)
            eventData.pointerPress = gameObject;
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        MyGrid.gridTiles.Exit(this);
        eventData.pointerPress = null;
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (MyGrid.gridTiles.drag == false && eventData.button == PointerEventData.InputButton.Left)
            MyGrid.gridTiles.Down();
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        print(gameObject.name + $", {transform.position.x}, {transform.position.z}");
        if (eventData.button == PointerEventData.InputButton.Left)
            MyGrid.gridTiles.Up();
        else
            MyGrid.gridTiles.BreakAction();
    }

    /// <summary>
    /// Creates the info window, if first is false only updates the info.
    /// </summary>
    /// <param name="first">Prepare the window.</param>
    /// <returns>info window reference</returns>
    public virtual InfoWindow OpenWindow(bool setUp = false)
    {
        if (!selected)
            return null;
        InfoWindow info = GameObject.FindWithTag("Info").transform.GetChild(0).GetComponent<InfoWindow>();
        info.gameObject.SetActive(true);
        return info;
    }


    /// <summary>
    /// Fills the jobSave with id and object type.
    /// </summary>
    /// <param name="jobSave">The value to be filled.</param>
    public virtual void GetID(JobSave jobSave)
    {
        jobSave.objectId = id;
        jobSave.objectType = typeof(ClickableObject);
    }

    public virtual string PrintText()
    {
        return "_";
    }

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

    protected void CreateNewId(List<int> clickables) // creates a random int
    {
        do
        {
            id = Random.Range(0, int.MaxValue);
        } while (clickables.Where(q => q == id).Count() > 0);
        return;
    }
}
