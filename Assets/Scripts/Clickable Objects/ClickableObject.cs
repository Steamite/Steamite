using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody))]
public class ClickableObject : MonoBehaviour, 
    IPointerEnterHandler, IPointerExitHandler, 
    IPointerDownHandler, IPointerUpHandler, IUpdatable
{
    [HideInInspector]
    public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

    public bool selected = false;
    public int id = -1;

    #region Object Operations
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;
        else if (((ClickableObject)obj).id == id && ((ClickableObject)obj).name == name)
            return true;
        return false;
    }

    public override int GetHashCode() { return base.GetHashCode(); }
    #endregion

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
            id = UnityEngine.Random.Range(0, int.MaxValue);
        } while (clickables.Where(q => q == id).Count() > 0);
        return;
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
    /// Sets the header text and returns info window reference.
    /// </summary>
    /// <param name="first">Prepare the window.</param>
    /// <returns>Info window reference, if the object is not selected throwns an error.</returns>
    public virtual InfoWindow OpenWindow()
    {
        if (selected)
        {
            InfoWindow info = SceneRefs.infoWindow;
            info.header.text = name;
            return info;
        }
        throw new ArgumentException();
    }

    /// <summary>
    /// Must be called after updating a bindable parameter. <br/>
    /// Has effect only if there is an active binding. <br/>
    /// Add <b>[CreateProperty]</b> to mark it.
    /// </summary>
    /// <param name="property">Name of the property, not field.</param>
    public void UpdateWindow(string property = "")
    {
        propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
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
}
