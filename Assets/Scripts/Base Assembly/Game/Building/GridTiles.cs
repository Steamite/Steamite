using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Handles different control states for <see cref="GridTiles"/>.</summary>
public enum ControlMode
{
    /// <summary>Basic inspection mode.</summary>
    Nothing,
    /// <summary>Building deconstruction mode.</summary>
    Deconstruct,
    /// <summary>Rock digging mode.(If started on marked Rocks, unmarks insted)</summary>
    Dig,
    /// <summary>Building placement mode.</summary>
    Build
}

/// <summary>Handles Input on the game Grid.</summary>
public class GridTiles : MonoBehaviour
{
    #region Variables
    [SerializeField] MultiSelect multiSelect;
    //bool buildingPlaced = false;
    /// <summary>Raycast mask for building.</summary>
    public LayerMask buildingMask;
    /// <summary>Default raycast mask for the rest of time.</summary>
    public LayerMask defaultMask;
    public LayerMask pipeMask;

    /// <summary>Current active control mode.</summary>
    public ControlMode ActiveControl { get; private set; } = ControlMode.Nothing;
    [SerializeField] InputAction shiftKey;

    /// <summary>Last mouse position.</summary>
    public GridPos activePos;
    /// <summary>Starting mouse position when dragging.</summary>

    /// <summary>If the mouse is down and trying to drag.</summary>
    public bool drag = false;
    /// <summary>Drag started on a marked tile.</summary>
    public bool deselect = false;

    
    /// <summary>Currently selected building for construction.</summary>
    [Header("Tilemaps")] Building blueprintPrefab;

    /// <summary>Changed from <see cref="BuildMenu"/></summary>
    public Building BuildPrefab
    {
        get => blueprintPrefab;
        set
        {
            blueprintPrefab = value;
            if (value == null)
            {
                if (blueprintInstance != null)
                    ChangeSelMode(ControlMode.Nothing);
            }
            else
                ChangeSelMode(ControlMode.Build);
        }
    }

    /// <summary>Building that's currently beeing placed.</summary>
    Building blueprintInstance;
    public Building BlueprintInstance
    {
        get => blueprintInstance;
        set
        {
            MyGrid.GetOverlay().DestroyBuilingTiles();
            blueprintInstance = value;
            if (value == null)
            {
                DeselectBuildingButton?.Invoke();
                ChangeSelMode(ControlMode.Nothing);
            }
        }
    }
    public Action DeselectBuildingButton;


    /// <summary>Clicked(selected) object.</summary>
    public ClickableObject clickedObject;
    /// <summary>Last object with mouse contact.</summary>
    public ClickableObject activeObject;

    /// <summary>List of all usable cursors.</summary>
    [Tooltip("used to help determine control states")] public Texture2D[] cursors;
    /// <summary>Basic highlight color(for selection).</summary>
    public Color highlight = Color.white / 3;

    public Color ToBeDugColor => multiSelect.toBeDugColor;
    #endregion

    /// <summary>Called when AltTabing from the game.</summary>
    void OnApplicationFocus(bool focus)
    {
        if (!focus)
        {
            Exit(activeObject);
            activeObject = null;
        }
    }

    #region Mouse Events
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

                Rock r = enterObject.GetComponent<Rock>();
                if (r && r.toBeDug) // if rock is to be dug
                    c += ToBeDugColor; // DUGCOLOR
                else
                {
                    Building b = enterObject.GetComponent<Building>();
                    if (b)
                    {
                        if (b.deconstructing)
                            c += Color.red / 2;/*
                        else if (!b.constructed)
                            c +=;*/
                    }
                }
                break;
            case ControlMode.Deconstruct:
                Building _b = enterObject.GetComponent<Building>();
                if (_b)
                    if (_b.deconstructing)
                        c = Color.red / 2;
                    else
                        c = Color.red;
                else
                    return;
                break;
            case ControlMode.Dig:
                Rock _r = enterObject.GetComponent<Rock>();
                if (drag)
                {
                    multiSelect.CalcTiles(activePos);
                    return;
                }
                else if (_r)
                {
                    if (_r.toBeDug)
                    {
                        c = Color.red;
                    }
                    else
                    {
                        c = Color.yellow;
                    }
                }
                break;
            case ControlMode.Build:
                if (drag)
                {
                    multiSelect.CalcPipes(activePos, blueprintPrefab as Pipe);
                    return;
                }
                else
                {
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
                }
                return;
        }
        enterObject.Highlight(c);
    }

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
                Rock r = exitObject.GetComponent<Rock>();
                Building b = exitObject.GetComponent<Building>();
                Pipe pipe = exitObject.GetComponent<Pipe>();
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
                Building _b = exitObject.GetComponent<Building>();
                if (_b && _b.deconstructing)
                    c = Color.red * 0.75f;
                else
                    c = new();
                break;
            case ControlMode.Dig:
                Rock _r = exitObject.GetComponent<Rock>();
                if (_r)
                {
                    if (drag)
                        return;
                    else if (_r.toBeDug)
                        c = ToBeDugColor;
                    else
                        c = new();
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
            DeselectObjects();
            Enter(temp);
            return;
        }
        Color c = new();
        Material[] m = activeObject.GetComponentsInChildren<MeshRenderer>().Where(q => q != null).Select(q => q.material).ToArray();
        switch (ActiveControl)
        {
            case ControlMode.Nothing:
                Rock r = activeObject.GetComponent<Rock>();
                if (clickedObject)
                {
                    Rock activeRock = clickedObject.GetComponent<Rock>();
                    if (activeRock && activeRock.toBeDug)
                        c = ToBeDugColor;
                    clickedObject.Highlight(c);
                    clickedObject.selected = false;
                }
                if (r && r.toBeDug) // rock to be dug
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
                Selection.activeObject = clickedObject.gameObject;
#endif
                break;
            case ControlMode.Deconstruct:
                Building b = activeObject.GetComponent<Building>();
                if (b)
                {
                    b.OrderDeconstruct();
                    if (b && !b.deconstructing)
                        c = Color.red;
                    else
                        c = Color.red / 2;
                    b.Highlight(c);
                }
                break;
            case ControlMode.Dig:
                Rock _r = activeObject.GetComponent<Rock>();
                if (_r)
                {
                    multiSelect.InitDig(
                        new GridPos(activePos.x, activePos.y, activePos.z),
                        _r);
                    drag = true;
                }
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
                if (blueprintPrefab is Pipe && (blueprintInstance == null || blueprintInstance.CanPlace()))
                {
                    if (drag == false)
                    {
                        multiSelect.InitPipes(
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
                        if(multiSelect.ClickPipes(activePos))
                            BlueprintInstance = null;
                    }

                }
                else if (blueprintInstance.CanPlace())
                {
                    blueprintInstance.PlaceBuilding();
                    if (shiftKey.IsInProgress() && MyRes.CanAfford(blueprintPrefab.Cost))
                    {
                        Blueprint();
                    }
                    else
                    {
                        BlueprintInstance = null;
                    }
                }
                else
                {
                    Debug.LogWarning("Can't place here!!");
                }
                break;
            case ControlMode.Dig:
                multiSelect.DigMark(activePos, activeObject as Rock);
                drag = false;
                Enter(activeObject);
                break;
        }
    }

    #endregion Mouse Events

    #region Multiselecting  
    
    #endregion Multiselecting

    #region Colors
    /// <summary>
    /// Highlights all materials on toBeChanged to c.
    /// </summary>
    /// <param name="c"></param>
    /// <param name="toBeChanged"></param>
    /*public void Highlight(Color c, GameObject toBeChanged)
    {
        (toBeChanged.GetComponent<ClickableObject>()).Highlight(c);
    }*/
    #endregion

    #region Control switching
    /// <summary>
    /// Called on press of right mouse button.
    /// </summary>
    public void BreakAction()
    {
        if (multiSelect.Break())
        {
            ChangeSelMode(ControlMode.Nothing);
            drag = false;
        }
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

    /// <summary>
    /// Changes the current tool mod, and manages transitions betwean them.
    /// </summary>
    /// <param name="mode"></param>
    public void ChangeSelMode(ControlMode mode)
    {
        if (mode == ActiveControl && mode != ControlMode.Nothing)
        {
            if (ActiveControl == ControlMode.Build)
            {
                if (blueprintPrefab.objectName == blueprintInstance.objectName)
                {
                    ChangeSelMode(ControlMode.Nothing);
                }
                else
                {
                    DestroyBlueprint(false);
                    Blueprint();
                    return;
                }
            }
            ChangeSelMode(ControlMode.Nothing);
        }
        else
        {
            switch (ActiveControl)
            {
                case ControlMode.Deconstruct:
                    ActiveControl = ControlMode.Nothing;
                    break;
                case ControlMode.Dig:
                    multiSelect.ClearDig();
                    drag = false;
                    ActiveControl = ControlMode.Nothing;
                    break;
                case ControlMode.Build:
                    SceneRefs.CameraSceneMover.SetRaycastMask(defaultMask);
                    if (drag)
                    {
                        multiSelect.ClearPipes();
                        DeselectBuildingButton?.Invoke();
                        drag = false;
                    }
                    else if (blueprintInstance)
                        DestroyBlueprint(true);
                    shiftKey.Disable();
                    break;
            }
            DeselectObjects();
            bool visible = true;
            Texture2D cur = null;
            Vector2 vec = new();
            ActiveControl = mode;
            switch (mode)
            {
                case ControlMode.Nothing:
                    cur = default;
                    vec = Vector2.zero;
                    Enter(activeObject);
                    break;
                case ControlMode.Deconstruct:
                    cur = cursors[0];
                    vec = new(15f, 15f);
                    Enter(activeObject);
                    break;
                case ControlMode.Dig:
                    cur = cursors[1];
                    vec = new(1, 16f);
                    Enter(activeObject);
                    break;
                case ControlMode.Build:
                    cur = default;
                    vec = Vector2.zero;
                    if (blueprintPrefab is Pipe)
                        SceneRefs.CameraSceneMover.SetRaycastMask(pipeMask);
                    else
                        SceneRefs.CameraSceneMover.SetRaycastMask(buildingMask);
                    Blueprint();
                    shiftKey.Enable();
                    break;
            }
            if (visible)
            {
                Cursor.SetCursor(cur, vec, CursorMode.Auto);
            }
        }
    }
    #endregion


    /// <summary>
    /// Instantiates and sets a copy of a building prefab.
    /// </summary>
    void Blueprint()
    {
        Quaternion q = new();
        if (blueprintInstance)
            q = new(blueprintInstance.transform.rotation.x, blueprintInstance.transform.rotation.y, blueprintInstance.transform.rotation.z, blueprintInstance.transform.rotation.w);
        GridPos gp = blueprintPrefab.blueprint.moveBy.Rotate(blueprintPrefab.transform.eulerAngles.y);
        gp = new(
            activePos.x + gp.x,
            (MyGrid.currentLevel * ClickableObjectFactory.LEVEL_HEIGHT) +
                (blueprintPrefab is Pipe
                ? ClickableObjectFactory.PIPE_OFFSET
                : ClickableObjectFactory.BUILD_OFFSET),
            activePos.z + gp.z);

        blueprintInstance = Instantiate(
            blueprintPrefab,
            new Vector3(gp.x, gp.y, gp.z),
            q,
            blueprintPrefab is Pipe
                ? MyGrid.FindLevelPipes()
                : MyGrid.FindLevelBuildings());
        if (blueprintInstance is IFluidWork)
            ((IFluidWork)blueprintInstance).CreatePipes();
        blueprintInstance.GetRenderComponents();

        blueprintInstance.maximalProgress = blueprintInstance.CalculateMaxProgress();
        blueprintInstance.ChangeRenderMode(true);
        blueprintInstance.Highlight(blueprintInstance.CanPlace() ? Color.blue : Color.red);
    }

    /// <summary>
    /// If the object was selected unselect.(Happens when the object is destroyed)
    /// </summary>
    /// <param name="cO"></param>
    public void DestroyUnselect(ClickableObject cO)
    {
        multiSelect.RemoveFromMarked(cO);
        Exit(cO);
        if (activeObject && activeObject == cO)
            activeObject = null;
        if (cO.selected)
        {
            DeselectObjects();
        }
    }

    /// <summary>
    /// Destroys the blueprint object and overlaygroup.
    /// </summary>
    /// <param name="forgetInstance">If true removes the instance.</param>
    void DestroyBlueprint(bool forgetInstance)
    {
        if (blueprintInstance is Pipe)
        {
            Pipe pipe = blueprintInstance as Pipe;
            for (int i = 0; i < 4; i++)
                pipe.DisconnectPipe(i, true);
        }
        else if (blueprintInstance is IFluidWork)
        {
            (blueprintInstance as IFluidWork).DisconnectFromNetwork();
        }
        Destroy(blueprintInstance.gameObject);
        if (forgetInstance)
            BlueprintInstance = null;
        else
            MyGrid.GetOverlay().DestroyBuilingTiles();
    }
}