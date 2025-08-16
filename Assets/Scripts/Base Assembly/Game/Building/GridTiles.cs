using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>Handles different control states for <see cref="GridTiles"/>.</summary>
public enum ControlMode
{
    /// <summary>Basic inspection mode.</summary>
    nothing,
    /// <summary>Building deconstruction mode.</summary>
    deconstruct,
    /// <summary>Rock digging mode.(If started on marked Rocks, unmarks insted)</summary>
    dig,
    /// <summary>Building placement mode.</summary>
    build
}

/// <summary>Handles Input on the game Grid.</summary>
public class GridTiles : MonoBehaviour
{
    #region Variables
    bool buildingPlaced = false;
    /// <summary>Raycast mask for building.</summary>
    public LayerMask buildingMask;
    /// <summary>Default raycast mask for the rest of time.</summary>
    public LayerMask defaultMask;
    public LayerMask pipeMask;

    /// <summary>Current active control mode.</summary>
    public ControlMode activeControl { get; private set; } = ControlMode.nothing;
    [SerializeField] InputAction shiftKey;

    /// <summary>Last mouse position.</summary>
    public GridPos activePos;
    /// <summary>Starting mouse position when dragging.</summary>
    public GridPos startPos;

    /// <summary>If the mouse is down and trying to drag.</summary>
    public bool drag = false;
    /// <summary>Drag started on a marked tile.</summary>
    public bool deselect = false;

    /// <summary>Tiles marked while dragging.</summary>
    public List<ClickableObject> tempMarkedTiles;
    List<List<ClickableObject>> markedTiles = new();
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
                    ChangeSelMode(ControlMode.nothing);
            }
            else
                ChangeSelMode(ControlMode.build);
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
                ChangeSelMode(ControlMode.nothing);
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
    /// <summary>Color for selecting what do dig.</summary>
    public Color toBeDugColor = (Color.yellow + Color.red) / 2;
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
        if (activeControl == ControlMode.nothing && activeObject != null)
            Exit(activeObject);
        activeObject = enterObject;
        activePos = enterObject.GetPos();
        switch (activeControl)
        {
            case ControlMode.nothing:

                if (activeObject.selected)// if active
                    c = highlight * 3; // WHITE
                else
                    c = highlight; // WHITE / 3

                Rock r = enterObject.GetComponent<Rock>();
                if (r && r.toBeDug) // if rock is to be dug
                    c += toBeDugColor; // DUGCOLOR
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
            case ControlMode.deconstruct:
                Building _b = enterObject.GetComponent<Building>();
                if (_b)
                    if (_b.deconstructing)
                        c = Color.red / 2;
                    else
                        c = Color.red;
                else
                    return;
                break;
            case ControlMode.dig:
                Rock _r = enterObject.GetComponent<Rock>();
                if (drag)
                {
                    CalcTiles();
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
            case ControlMode.build:
                if (drag)
                {
                    CalcPipes();
                    MyGrid.GetOverlay().MovePlacePipeOverlay(activePos, false);
                    return;
                }
                else
                {
                    GridPos grid = MyGrid.Rotate(blueprintInstance.blueprint.moveBy, blueprintInstance.transform.eulerAngles.y);
                    blueprintInstance.transform.position = new(
                        activePos.x + grid.x,
                        (MyGrid.currentLevel * ClickableObjectFactory.LEVEL_HEIGHT) +
                            (blueprintInstance is Pipe
                            ? ClickableObjectFactory.PIPE_OFFSET
                            : ClickableObjectFactory.BUILD_OFFSET),
                        activePos.z + grid.z);
                    c = blueprintInstance.CanPlace() ? Color.blue : Color.red;
                    HighLight(c, blueprintInstance.gameObject);
                }
                return;
        }
        HighLight(c, enterObject.gameObject);
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
        switch (activeControl)
        {
            case ControlMode.nothing:
                Rock r = exitObject.GetComponent<Rock>();
                Building b = exitObject.GetComponent<Building>();
                Pipe pipe = exitObject.GetComponent<Pipe>();
                if (exitObject.selected)
                    c = highlight * 2;
                else
                    c = new();
                if (r && r.toBeDug)
                    c += toBeDugColor;
                else if (b && b.deconstructing)
                    c += Color.red / 2;
                else if (pipe)
                {
                    HighLight(c, pipe.gameObject);
                    return;
                }

                if (exitObject == activeObject)
                    activeObject = null;
                break;
            case ControlMode.deconstruct:
                Building _b = exitObject.GetComponent<Building>();
                if (_b && _b.deconstructing)
                    c = Color.red * 0.75f;
                else
                    c = new();
                break;
            case ControlMode.dig:
                Rock _r = exitObject.GetComponent<Rock>();
                if (_r)
                {
                    if (drag)
                        return;
                    else if (_r.toBeDug)
                        c = toBeDugColor;
                    else
                        c = new();
                }
                break;
            case ControlMode.build:
                return;
        }
        HighLight(c, exitObject.gameObject);
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
        switch (activeControl)
        {
            case ControlMode.nothing:
                Rock r = activeObject.GetComponent<Rock>();
                if (clickedObject)
                {
                    Rock activeRock = clickedObject.GetComponent<Rock>();
                    if (activeRock && activeRock.toBeDug)
                        c = toBeDugColor;
                    HighLight(c, clickedObject.gameObject);
                    clickedObject.selected = false;
                }
                if (r && r.toBeDug) // rock to be dug
                    c = toBeDugColor + highlight * 2; // YELLOW + RED
                else
                    c = highlight * 3; // WHITE
                HighLight(c, activeObject.gameObject);

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
            case ControlMode.deconstruct:
                Building b = activeObject.GetComponent<Building>();
                if (b)
                {
                    b.OrderDeconstruct();
                    if (b && !b.deconstructing)
                        c = Color.red;
                    else
                        c = Color.red / 2;
                    HighLight(c, b.gameObject);
                }
                break;
            case ControlMode.dig:
                Rock _r = activeObject.GetComponent<Rock>();
                if (_r)
                {
                    startPos = new(activePos.x, activePos.y, activePos.z);
                    drag = true;
                    deselect = _r.toBeDug ? true : false;
                    tempMarkedTiles = new() { _r };
                }
                break;
            case ControlMode.build:
                
                break;
        }
    }

    /// <summary>
    /// Called when mouse presses up the ClickableObject collider.
    /// </summary>
    public void Up()
    {
        switch (activeControl)
        {
            case ControlMode.nothing:
            case ControlMode.deconstruct:
                // nothing
                break;
            case ControlMode.build:
                if (blueprintPrefab is Pipe && (blueprintInstance == null || blueprintInstance.CanPlace()))
                {
                    if(drag == false)
                    {
                        MyGrid.GetOverlay().MovePlacePipeOverlay(activePos, true);
                        drag = true;
                        tempMarkedTiles = new() { blueprintInstance };
                        GridPos gridPos = activeObject.GetPos();
                        MyGrid.SetGridItem(gridPos, blueprintInstance, true);
                        blueprintInstance = null;
                        startPos = activePos;
                    }
                    else
                    {
                        if (tempMarkedTiles.Count > 1)
                        {
                            MarkPipeCheckpoint();
                        }
                        else
                        {
                            foreach (Building b in markedTiles.SelectMany(q => q).Union(tempMarkedTiles))
                            {
                                if (MyRes.CanAfford(b.Cost))
                                    b.PlaceBuilding();
                                else
                                    b.DestoyBuilding();
                            }
                            buildingPlaced = true;
                            markedTiles.Clear();
                            tempMarkedTiles.Clear();
                            BlueprintInstance = null;
                        }
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
            case ControlMode.dig:
                PrepDig();
                tempMarkedTiles = new();
                break;
        }
    }

    #endregion Mouse Events

    #region Multiselecting  
    /// <summary>
    /// Called when canceling drag, changes highlight of all rocks in markedTiles.
    /// </summary>
    void ClearMarks()
    {
        if (tempMarkedTiles != null)
        {
            foreach (Rock r in tempMarkedTiles)
            {
                Color c = new();
                if (r.toBeDug)
                    c = (Color.yellow + Color.red) / 2;
                HighLight(c, r.gameObject);
            }
        }
    }
    /// <summary>
    /// ReMarks all rock in range from start to activePos. 
    /// </summary>
    void CalcTiles()
    {
        ClearMarks();
        List<ClickableObject> rocks = new();
        float x = (Mathf.FloorToInt(startPos.x) - activePos.x) / 2f;
        float z = (Mathf.FloorToInt(startPos.z) - activePos.z) / 2f;
        rocks.AddRange(Physics.OverlapBox(new Vector3(startPos.x - x, (startPos.y * 2) + ClickableObjectFactory.ROCK_OFFSET, startPos.z - z), new(Mathf.Abs(x), 0.5f, Mathf.Abs(z))).Where(q => q.GetComponent<Rock>() != null).Select(q => q.GetComponent<Rock>()).ToList());
        List<ClickableObject> filtered = rocks.ToList();
        List<Rock> toBeDug = SceneRefs.JobQueue.toBeDug;
        foreach (Rock g in rocks)
        {
            if (!deselect && toBeDug.Contains(g))
            {
                filtered.Remove(g);
            }
            HighLight(deselect ? (Color.red / 2) : toBeDugColor, g.gameObject);
        }
        tempMarkedTiles = filtered;
    }

    /// <summary>
    /// Creates/Deletes pipes to copy the shortest path from startPos to activePos
    /// </summary>
    void CalcPipes()
    {
        Transform pipes = MyGrid.FindLevelPipes(startPos.y / 2);
        List<GridPos> path = tempMarkedTiles.Select(q => q.GetPos()).ToList();
        int i = path.IndexOf(activePos);
        if(i == -1)
        {
            if(MyGrid.GetGridItem(activePos, true) == null && MyGrid.GetGridItem(activePos) is Road)
            {
                GridPos lastPos = path[^1];
                if (Math.Abs(lastPos.x - activePos.x) + Math.Abs(lastPos.z - activePos.z) > 1)
                {
                    List<GridPos> partPath = PathFinder.FindPath(lastPos, activePos, typeof(Road));
                    foreach (var tile in partPath)
                    {
                        if(MyGrid.GetGridItem(tile, true) == null)
                        {
                            AddPipe(tile, pipes);
                        }
                        else
                        {
                            int j = path.IndexOf(tile);
                            if (j > -1)
                            {
                                for (int k = tempMarkedTiles.Count-1; k > j; k--)
                                {
                                    (tempMarkedTiles[k] as Building).DestoyBuilding();
                                    tempMarkedTiles.RemoveAt(k);
                                }
                            }
                        }
                    }
                }
                else
                    AddPipe(activePos, pipes);


            }
        }
        else
        {
            for(int j = tempMarkedTiles.Count-1; j > i; j--)
            {
                (tempMarkedTiles[j] as Building).DestoyBuilding();
                tempMarkedTiles.RemoveAt(j);
            }
        }
    }
    void AddPipe(GridPos pos, Transform pipes)
    {
        Pipe pipe = Instantiate(blueprintPrefab as Pipe, pos.ToVec(ClickableObjectFactory.PIPE_OFFSET), Quaternion.identity, pipes);
        pipe.maximalProgress = pipe.CalculateMaxProgress();
        pipe.GetRenderComponents();
        pipe.ChangeRenderMode(true);
        HighLight(pipe.CanPlace(false) && MyRes.CanAfford(pipe.Cost * (tempMarkedTiles.Count + 1 + markedTiles.SelectMany(q => q).Count())) ? Color.blue : Color.red, pipe.gameObject);
        MyGrid.SetGridItem(pos, pipe, true);
        tempMarkedTiles.Add(pipe);
    }

    public void MarkPipeCheckpoint()
    {
        if(activeControl == ControlMode.build && drag == true && tempMarkedTiles.Count > 1)
        {
            markedTiles.Add(tempMarkedTiles.ToList());
            tempMarkedTiles.Clear();
            tempMarkedTiles.Add(markedTiles.Last().Last());
            MyGrid.GetOverlay().AddCheckPointTile(activePos);
        }
    }
    #endregion Multiselecting

    #region Colors
    /// <summary>
    /// HighLights all materials on toBeChanged to c.
    /// </summary>
    /// <param name="c"></param>
    /// <param name="toBeChanged"></param>
    public void HighLight(Color c, GameObject toBeChanged)
    {
        foreach (Material material in
            toBeChanged.GetComponentsInChildren<MeshRenderer>()
            .Where(q => q).SelectMany(q => q.materials)
            .Union(toBeChanged.GetComponentsInChildren<SkinnedMeshRenderer>()
            .Where(q => q).SelectMany(q => q.materials)))
        {
            material.SetColor("_EmissionColor", c);
        }
    }
    #endregion

    #region Control switching
    /// <summary>
    /// Called on press of right mouse button.
    /// </summary>
    public void BreakAction(bool force = false)
    {
        if(force == false && markedTiles.Count > 0)
        {
            foreach (Building o in tempMarkedTiles.Skip(1))
            {
                o.DestoyBuilding();
            }
            MyGrid.GetOverlay().RemoveCheckPointTile(markedTiles.Count+1);
            tempMarkedTiles = markedTiles.Last();
            markedTiles.RemoveAt(markedTiles.Count - 1);
            SceneRefs.CameraSceneMover.MoveToPosition(tempMarkedTiles.Last().GetPos(), true);
        }
        else
        {
            ChangeSelMode(ControlMode.nothing);
            drag = false;
        }
    }

    /// <summary>
    /// Removes clickedObject from selection.
    /// </summary>
    public void DeselectObjects()
    {
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
        if (mode == activeControl && mode != ControlMode.nothing)
        {
            if (activeControl == ControlMode.build)
            {
                if (blueprintPrefab.objectName == blueprintInstance.objectName)
                {
                    ChangeSelMode(ControlMode.nothing);
                }
                else
                {
                    DestroyBlueprint(false);
                    Blueprint();
                    return;
                }
            }
            ChangeSelMode(ControlMode.nothing);
        }
        else
        {
            switch (activeControl)
            {
                case ControlMode.deconstruct:
                    activeControl = ControlMode.nothing;
                    break;
                case ControlMode.dig:
                    foreach (Rock r in tempMarkedTiles)
                    {
                        HighLight(new(), r.gameObject);
                    }
                    tempMarkedTiles = new();
                    startPos = null;
                    drag = false;
                    activeControl = ControlMode.nothing;
                    break;
                case ControlMode.build:
                    SceneRefs.CameraSceneMover.SetRaycastMask(defaultMask);
                    if (drag)
                    {
                        foreach (Pipe pipe in tempMarkedTiles.Union(markedTiles.SelectMany(q => q)))
                        {
                            blueprintInstance = pipe;
                            DestroyBlueprint(false);
                        }
                        blueprintInstance = null;
                        DeselectBuildingButton?.Invoke();
                        markedTiles.Clear();
                        tempMarkedTiles.Clear();
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
            activeControl = mode;
            switch (mode)
            {
                case ControlMode.nothing:
                    cur = default;
                    vec = Vector2.zero;
                    Enter(activeObject);
                    break;
                case ControlMode.deconstruct:
                    cur = cursors[0];
                    vec = new(15f, 15f);
                    break;
                case ControlMode.dig:
                    cur = cursors[1];
                    vec = new(1, 16f);
                    break;
                case ControlMode.build:
                    cur = default;
                    vec = Vector2.zero;
                    if(blueprintPrefab is Pipe)
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
        GridPos gp = MyGrid.Rotate(blueprintPrefab.blueprint.moveBy, blueprintPrefab.transform.eulerAngles.y);
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
        HighLight(blueprintInstance.CanPlace() ? Color.blue : Color.red, blueprintInstance.gameObject);
    }

    /// <summary>
    /// Changes the state of rocks in <see cref="tempMarkedTiles"/>, if the first one was marked, cancels them.<br/>
    /// Else marks orders their excavation.
    /// </summary>
    void PrepDig()
    {
        List<Rock> toBeDug = SceneRefs.JobQueue.toBeDug;
        drag = false;
        HumanUtil humans = transform.parent.parent.GetChild(2).GetComponent<HumanUtil>();
        if (deselect)
        {
            foreach (Rock markTile in tempMarkedTiles.Select(q => q.GetComponent<Rock>())) // removes to be dug
            {
                toBeDug.RemoveAll(q => q == markTile);
                markTile.toBeDug = false;
                HighLight(new(), markTile.gameObject);
                SceneRefs.JobQueue.CancelJob(JobState.Digging, markTile);
                markTile.Assigned?.SetJob(JobState.Free);
            }
        }
        else
        {
            foreach (var dig in toBeDug) // removes to be dug
            {
                tempMarkedTiles.RemoveAll(q => q == dig);
            }
            foreach (Rock tile in tempMarkedTiles)
            {
                toBeDug.Add(tile); // add rock
                tile.toBeDug = true;
                SceneRefs.JobQueue.AddJob(JobState.Digging, tile);
            }
        }
        tempMarkedTiles.Clear();
        deselect = false;
        Enter(activeObject);
    }

    /// <summary>
    /// If the object was selected unselect.(Happens when the object is destroyed)
    /// </summary>
    /// <param name="cO"></param>
    public void DestroyUnselect(ClickableObject cO)
    {
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