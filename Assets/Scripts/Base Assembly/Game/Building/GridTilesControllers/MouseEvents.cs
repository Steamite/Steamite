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
    public ClickableObject clickedObject;
    /// <summary>Last object with mouse contact.</summary>
    public ClickableObject activeObject;

    public GridPos activePos;


    /// <summary>If the mouse is down and trying to drag.</summary>
    public bool drag;

    ControlMode ActiveControl => gridTiles.ActiveControl;

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
        Color c = new();
        if (ActiveControl == ControlMode.Nothing && activeObject != null)
            Exit(activeObject);
        activeObject = enterObject;
        activePos = enterObject.GetPos();
        switch (ActiveControl)
        {
            case ControlMode.Nothing:

                if (activeObject.selected)// if active
                    c = highlight * 3; // WHITE
                else
                    c = highlight; // WHITE / 3

                Rock r = enterObject as Rock;
                if (r && r.toBeDug) // if rock is to be dug
                    c += ToBeDugColor; // DUGCOLOR
                else
                {
                    Building b = enterObject as Building;
                    if (b)
                    {
                        if (b.deconstructing)
                            c += Color.red / 2;
                        /*
                    else if (!b.constructed)
                        c +=;*/
                    }
                }
                break;
            case ControlMode.Deconstruct:
                Building _b = enterObject as Building;
                if (_b)
                    if (_b.deconstructing)
                        c = Color.red / 2;
                    else
                        c = Color.red;
                else
                    return;
                break;
            case ControlMode.Dig:
                Rock _r = enterObject as Rock;
                if (drag)
                {
                    gridTiles.CalcTiles(activePos);
                    return;
                }
                else if (!_r)
                    return;
                
                if (_r.toBeDug)
                {
                    c = Color.red;
                }
                else
                {
                    c = Color.yellow;
                }
                break;
            case ControlMode.Upgrade:
                Building building = enterObject as Building;
                if (building == null)
                    return;

                if (building.CanUpgrade())
                    c = Color.darkGreen;
                else
                    c = Color.red;
                break;
            case ControlMode.Build:
                Building blueprintPrefab = gridTiles.BlueprintPrefab;
                Building blueprintInstance = gridTiles.BlueprintInstance;

                if (drag)
                {
                    gridTiles.CalcPipes(activePos, blueprintPrefab as Pipe);
                    return;
                }

                GridPos grid = blueprintInstance.blueprint.moveBy.Rotate(blueprintInstance.transform.eulerAngles.y);
                blueprintInstance.transform.position = new(
                    activePos.x + grid.x,
                    (MyGrid.currentLevel * ClickableObjectFactory.LEVEL_HEIGHT) +
                        (blueprintInstance is Pipe
                        ? ClickableObjectFactory.PIPE_OFFSET
                        : ClickableObjectFactory.BUILD_OFFSET),
                    activePos.z + grid.z);
                c = blueprintInstance.CanPlace() ? Color.blue : Color.red;
                blueprintInstance.Highlight(c);
                return;
        }
        enterObject.Highlight(c);
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
        Color c = new();
        switch (ActiveControl)
        {
            case ControlMode.Nothing:
                Rock r = exitObject as Rock;
                Building b = exitObject as Building;
                Pipe pipe = exitObject as Pipe;
                if (exitObject.selected)
                    c = highlight * 2;
                else
                    c = new();
                if (r && r.toBeDug)
                    c += ToBeDugColor;
                else if (b && b.deconstructing)
                    c += Color.red / 2;
                else if (pipe)
                {
                    pipe.Highlight(c);
                    return;
                }

                if (exitObject == activeObject)
                    activeObject = null;
                break;
            case ControlMode.Deconstruct:
                Building _b = exitObject as Building;
                if (_b && _b.deconstructing)
                    c = Color.red * 0.75f;
                break;
            case ControlMode.Dig:
                Rock _r = exitObject as Rock;
                if (_r)
                {
                    if (drag)
                        return;
                    else if (_r.toBeDug)
                        c = ToBeDugColor;
                }
                break;
            case ControlMode.Build:
                return;
        }
        exitObject.Highlight(c);
    }

    /// <summary>
    /// Called when mouse presses down the ClickableObject collider.
    /// </summary>
    public void Down()
    {
        if (activeObject == null)
            return;
        else if (activeObject == clickedObject)
        {
            ClickableObject temp = clickedObject;
            gridTiles.DeselectObjects();
            Enter(temp);
            return;
        }
        Color c = new();
        Material[] m = activeObject.GetComponentsInChildren<MeshRenderer>().Where(q => q != null).Select(q => q.material).ToArray();

        Building building;
        Rock rock;
        switch (ActiveControl)
        {
            case ControlMode.Nothing:
                rock = activeObject as Rock;
                if (clickedObject)
                {
                    Rock activeRock = clickedObject as Rock;
                    if (activeRock && activeRock.toBeDug)
                        c = ToBeDugColor;
                    clickedObject.Highlight(c);
                    clickedObject.selected = false;
                }
                if (rock && rock.toBeDug) // rock to be dug
                    c = ToBeDugColor + highlight * 2; // YELLOW + RED
                else
                    c = highlight * 3; // WHITE
                activeObject.Highlight(c);

                // DEBUG_Binding Working entrypoint
                // This happens when you click an object in the level.
                // You need to have "nothing" selection mode (white highliting, If you dont press right mouse button to get there).
                clickedObject = activeObject;
                clickedObject.selected = true;
                clickedObject.OpenWindow();
#if UNITY_EDITOR
                if(select)
                    Selection.activeObject = clickedObject.gameObject;
#endif
                break;
            case ControlMode.Deconstruct:
                building = activeObject as Building;
                if (building)
                {
                    building.OrderDeconstruct();
                    if (building && !building.deconstructing)
                        c = Color.red;
                    else
                        c = Color.red / 2;
                    building.Highlight(c);
                }
                break;
            case ControlMode.Dig:
                rock = activeObject as Rock;
                if (rock)
                {
                    gridTiles.InitDig(rock);
                    drag = true;
                }
                break;
            case ControlMode.Upgrade:
                building = activeObject as Building;
                building.StartUpgrade();
                break;
            case ControlMode.Build:
                break;
        }
    }

    /// <summary>
    /// Called when mouse presses up the ClickableObject collider.
    /// </summary>
    public void Up()
    {
        switch (ActiveControl)
        {
            case ControlMode.Nothing:
            case ControlMode.Deconstruct:
                // nothing
                break;
            case ControlMode.Build:
                Building blueprintPrefab = gridTiles.BlueprintPrefab;
                Building blueprintInstance = gridTiles.BlueprintInstance;
                if (blueprintPrefab is Pipe && (blueprintInstance == null || blueprintInstance.CanPlace()))
                {
                    if (drag == false)
                    {
                        gridTiles.InitPipes(
                            new GridPos(activePos.x, activePos.y, activePos.z),
                            blueprintInstance as Pipe);
                        MyGrid.GetOverlay().MovePlacePipeOverlay(activePos, true);
                        drag = true;

                        GridPos gridPos = activeObject.GetPos();
                        MyGrid.SetGridItem(activePos, blueprintInstance, true);
                        blueprintInstance = null;
                    }
                    else
                    {
                        if (gridTiles.ClickPipes(activePos))
                            gridTiles.BlueprintInstance = null;
                    }

                }
                else if (blueprintInstance.CanPlace())
                {
                    blueprintInstance.PlaceBuilding();
                    if (gridTiles.shiftKey.IsInProgress() && MyRes.CanAfford(blueprintPrefab.Cost))
                    {
                        gridTiles.Blueprint();
                    }
                    else
                    {
                        gridTiles.BlueprintInstance = null;
                    }
                }
                else
                {
                    Debug.LogWarning("Can't place here!!");
                }
                break;
            case ControlMode.Dig:
                gridTiles.DigMark(activeObject as Rock);
                drag = false;
                Enter(activeObject);
                break;
        }
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
        }
        if (clickedObject)
        {
            clickedObject.selected = false;
            Exit(clickedObject);
            if (activeObject == null)
                activeObject = clickedObject;
            clickedObject = null;
            SceneRefs.InfoWindow.Close();
        }
    }
}
