using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

/// <summary>
/// The Base class for all objects in game.<br/>
/// </summary>
//[RequireComponent(typeof(Rigidbody))]
public abstract class ClickableObject : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler, IUpdatable
{
    [HideInInspector]
    [Obsolete("Use objectName not name", true)]
    public new string name;

    /// <summary>Bind event for updating.</summary>
    [HideInInspector]
    public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

    /// <summary>Object is beeing currently inspected.</summary>
    public bool selected = false;
    /// <summary>ID is a unique identifier for each group of objects.</summary>
    public int id = -1;

    public string objectName;

    #region Object Operations
    /// <summary>
    /// Compares by <see cref="id"/> and name.
    /// </summary>
    /// <param name="obj">Object to compare.</param>
    /// <returns>Comparison result.</returns>
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;
        else if (((ClickableObject)obj).id == id && ((ClickableObject)obj).objectName == objectName)
            return true;
        return false;
    }

    public override int GetHashCode() => base.GetHashCode();
    #endregion

    #region Basic Operations
    /// <summary>Should create a unique id for a new object in a list.</summary>
    public virtual void UniqueID() => id = -1;

    public override string ToString()
    {
        return $"{objectName}: {id}";
    }
    /// <summary>
    /// Calculates position on the grid.
    /// </summary>
    /// <returns>Calculated position.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public virtual GridPos GetPos() => throw new NotImplementedException();
    /// <summary>
    /// Fills the <paramref name="jobSave"/> with object id and type.
    /// </summary>
    /// <exception cref="NotImplementedException">Not Implemented</exception>
    public void GetID(JobSave jobSave)
    {
        jobSave.interestID = id;
        switch (this)
        {
            case Building:
                jobSave.interestType = JobSave.InterestType.B;
                break;
            case Rock:
                jobSave.interestType = JobSave.InterestType.R;
                break;
            case Chunk:
                jobSave.interestType = JobSave.InterestType.C;
                break;
        }
    }
    /// <summary>
    /// Keeps randomly creating new IDs, until it's unique.
    /// </summary>
    /// <param name="clickables">List of taken ids.</param>
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
    /// <summary>
    /// Triggers <see cref="GridTiles.Enter(ClickableObject)"/>.<br/>
    /// And marks this object as the origin of click, if dragging.
    /// </summary>
    /// <param name="eventData">Mouse data</param>
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        SceneRefs.gridTiles.Enter(this);
        if (SceneRefs.gridTiles.drag)
            eventData.pointerPress = gameObject;
    }
    /// <summary>
    /// Triggers <see cref="GridTiles.Exit(ClickableObject)"/>.<br/>
    /// And removes the click origin.
    /// </summary>
    /// <param name="eventData">Mouse data</param>
    public virtual void OnPointerExit(PointerEventData eventData)
    {
        SceneRefs.gridTiles.Exit(this);
        eventData.pointerPress = null;
    }
    /// <summary>
    /// If not dragging and the button pressed was the left one, triggers <see cref="GridTiles.Down()"/>.
    /// </summary>
    /// <param name="eventData">Mouse data</param>
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (SceneRefs.gridTiles.drag == false && eventData.button == PointerEventData.InputButton.Left)
            SceneRefs.gridTiles.Down();
    }
    /// <summary>
    /// If left button was released Triggers <see cref="GridTiles.Up()"/>.<br/>
    /// Else <see cref="GridTiles.BreakAction()"/>.
    /// </summary>
    /// <param name="eventData">Mouse data</param>
    public virtual void OnPointerUp(PointerEventData eventData)
    {
        //print(gameObject.name + $", {transform.position.x}, {transform.position.z}");
        if (eventData.button == PointerEventData.InputButton.Left)
            SceneRefs.gridTiles.Up();
        else
            SceneRefs.gridTiles.BreakAction();
    }
    #endregion Mouse Events

    #region Window
    /// <summary>
    /// Sets the header text and returns info window reference.<br/>
    /// </summary>
    /// <param name="first">Prepare the window.</param>
    /// <returns>Info window reference, if the object is not selected throwns an error.</returns>
    public virtual InfoWindow OpenWindow()
    {
        if (selected)
        {
            InfoWindow info = SceneRefs.infoWindow;
            info.header.text = objectName;
            return info;
        }
        Debug.LogWarning($"Object \"{this}\" not selected, why open window?");
        return null;
    }
    

    /// <summary>
    /// Must be called after updating a bindable parameter. <br/>
    /// Has effect only if there is an active binding. <br/>
    /// Add <b>[CreateProperty]</b> to mark it.
    /// </summary>
    /// <param name="property">Name of the property, not field.</param>
    public void UIUpdate(string property = "")
    {
        if (selected && propertyChanged == null)
            Debug.LogWarning($"no bindings {property}");
        propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
    }

    #endregion Window

    #region Saving
    /// <summary>
    /// Recursively calls down and fills the <paramref name="clickable"/>.
    /// </summary>
    /// <param name="clickable">Save data to fill.</param>
    /// <returns>Pointer to <paramref name="clickable"/>.</returns>
    public virtual ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        if (clickable == null)
            clickable = new();
        clickable.id = id;
        clickable.objectName = objectName;

        return clickable;
    }
    /// <summary>
    /// Recursively calls down and loads data.
    /// </summary>
    /// <param name="save">Save data to load from.</param>
    public virtual void Load(ClickableObjectSave save)
    {
        id = save.id;
        objectName = save.objectName;
    }

    #endregion Saving
    public bool HasActiveBinding() => propertyChanged != null;
}
