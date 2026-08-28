

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(GridTiles))]
public class MouseEvents : MonoBehaviour
{
    GridTiles gridTiles;

    /// <summary>Clicked(selected) object.</summary>
    public ClickableObject selectedObject;
    /// <summary>Last object with mouse contact.</summary>
    public ClickableObject activeObject;

    public GridPos activePos;


    /// <summary>If the mouse is down and trying to drag.</summary>
    public bool drag;

    GridTilesMode ActiveControl => gridTiles.ActiveControl;

    /// <summary>Basic highlight color(for selection).</summary>= Color.white / 3;
    public Color highlight;
#if UNITY_EDITOR
    [SerializeField] bool select;
#endif
    public Color ToBeDugColor => gridTiles.ToBeDugColor;


    private void Awake()
    {
        gridTiles = GetComponent<GridTiles>();
    }


    public void Enter() => Enter(activeObject);
    /// <summary>
    /// Called when mouse enters the ClickableObject collider.
    /// </summary>
    /// <param name="enterObject"></param>
    public void Enter(ClickableObject enterObject)
    {
        if (enterObject == null)
            return;
        if (ActiveControl is Nothing && activeObject != null)
            Exit(activeObject);

        activeObject = enterObject;
        activePos = enterObject.GetPos();

        ActiveControl.EnterObject(enterObject);
        
        //enterObject.Highlight(c);
    }

    public void Exit() => Exit(activeObject);
    /// <summary>
    /// Called when mouse leaves the ClickableObject collider.
    /// </summary>
    /// <param name="exitObject"></param>
    public void Exit(ClickableObject exitObject)
    {
        if (exitObject == null)
            return;
        ActiveControl.ExitObject(exitObject);
        //exitObject.Highlight(c);
    }

    /// <summary>
    /// Called when mouse presses down the ClickableObject collider.
    /// </summary>
    public void Down()
    {
        if (activeObject == null)
            return;
        else if (activeObject == selectedObject)
        {
            ClickableObject temp = selectedObject;
            gridTiles.DeselectObjects();
            Enter(temp);
            return;
        }
        ActiveControl.DownObject();
    }

    /// <summary>
    /// Called when mouse presses up the ClickableObject collider.
    /// </summary>
    public void Up()
    {
        ActiveControl.UpObject();
    }

    public void Clear()
    {
        Exit(activeObject);
        activeObject = null;
    }
    /// <summary>
    /// Removes clickedObject from selection.
    /// </summary>
    public void DeselectObjects()
    {
        if (activeObject)
        {
            var a = activeObject;
            Exit(activeObject);
            activeObject = a;
            SceneRefs.Overlays.overlay.ClearSelectedTiles();
        }
        if (selectedObject)
        {
            selectedObject.selected = false;
            Exit(selectedObject);
            if (activeObject == null)
                activeObject = selectedObject;
            selectedObject = null;
            InfoWindow.Window.Close();
        }
    }
}
