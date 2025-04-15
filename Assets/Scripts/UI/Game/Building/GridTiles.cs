using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

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
    /// <summary>Raycast mask for building.</summary>
    public LayerMask buildingMask;
    /// <summary>Default raycast mask for the rest of time.</summary>
    public LayerMask defaultMask;
    
    /// <summary>Currently selected building for construction.</summary>
    [Header("Tilemaps")] public Building buildingPrefab;
    /// <summary>Current active control mode.</summary>
    public ControlMode activeControl { get; private set; } = ControlMode.nothing;

    /// <summary>Last mouse position.</summary>
    public GridPos activePos;
    /// <summary>Starting mouse position when dragging.</summary>
    public GridPos startPos;

    /// <summary>If the mouse is down and trying to drag.</summary>
    public bool drag = false;
    /// <summary>Drag started on a marked tile.</summary>
    public bool deselect = false;

    /// <summary>Tiles marked while dragging.</summary>
    public List<ClickableObject> markedTiles;
    /// <summary>Building that's currently beeing placed.</summary>
    public Building buildBlueprint;

    /// <summary>Clicked(selected) object.</summary>
    public ClickableObject clickedObject;
    /// <summary>Last object with mouse contact.</summary>
    public ClickableObject activeObject;

    /// <summary>List of all usable cursors.</summary>
    [Tooltip("used to help determine control states")] public Texture2D[] cursors;
    /// <summary>Basic highlight color(for selection).</summary>
    public Color highlight = Color.white/3;
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
                    return;
                }
                else
                {
                    //Build __b = buildBlueprint.build;
                    GridPos grid = MyGrid.Rotate(buildBlueprint.blueprint.moveBy, buildBlueprint.transform.eulerAngles.y);
                    grid = new(activePos.x + grid.x, (MyGrid.currentLevel * ClickableObjectFactory.LEVEL_HEIGHT) + ClickableObjectFactory.BUILD_OFFSET, activePos.z + grid.z);
                    buildBlueprint.transform.position = new(grid.x, grid.y, grid.z);
                    c = buildBlueprint.CanPlace() ? Color.blue : Color.red;
                    HighLight(c, buildBlueprint.gameObject);
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
        Material[] m = exitObject.GetComponentsInChildren<MeshRenderer>().Select(q=> q.material).ToArray();
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
                    c += Color.red/2;
                else if (pipe)
                {
                    HighLight(c, pipe.gameObject);
                    return;
                }

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
        else if(activeObject == clickedObject)
        {
            DeselectObjects();
            Enter(activeObject);
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
                clickedObject = activeObject;
                clickedObject.selected = true;
                clickedObject.OpenWindow();
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
                    markedTiles = new() { _r };
                }
                break;
            case ControlMode.build:
                if (buildBlueprint.CanPlace())
                {
                    if (buildBlueprint.GetComponent<Pipe>())
                    {
                        drag = true;
                        markedTiles = new() { buildBlueprint};
                        GridPos gridPos = activeObject.GetPos();
                        MyGrid.SetGridItem(gridPos, buildBlueprint, true);
                        buildBlueprint.UniqueID();
                        buildBlueprint = null;
                    }
                    startPos = activePos;
                }
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

                if (drag)
                {
                    foreach (Building b in markedTiles.Select(q => q.GetComponent<Building>()))
                    {
                        if (b.id != -1)
                        {
                            b.PlaceBuilding(this);
                        }
                        else
                        {
                            b.DestoyBuilding();
                        }
                    }

                    markedTiles = new();
                    drag = false;
                    if (Input.GetButton("Shift"))
                    {
                        Blueprint();
                    }
                    else
                    {
                        ChangeSelMode(ControlMode.nothing);
                        buildBlueprint = null;
                    }
                }
                else if (buildBlueprint.CanPlace())
                {
                    buildBlueprint.PlaceBuilding(this);

                    if (Input.GetButton("Shift"))
                    {
                        Blueprint();
                    }
                    else
                    {
                        buildBlueprint = null;
                        ChangeSelMode(ControlMode.nothing);
                    }
                }
                else 
                {
                    Debug.LogWarning("Can't place here!!");
                }
                break;
            case ControlMode.dig:
                PrepDig();
                markedTiles = new();
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
        if (markedTiles != null)
        {
            foreach (Rock r in markedTiles)
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
        rocks.AddRange(Physics.OverlapBox(new Vector3(startPos.x - x, (startPos.y*2) + ClickableObjectFactory.ROCK_OFFSET, startPos.z - z), new(Mathf.Abs(x), 0.5f, Mathf.Abs(z))).Where(q => q.GetComponent<Rock>() != null).Select(q => q.GetComponent<Rock>()).ToList());
        List<ClickableObject> filtered = rocks.ToList();
        List<Rock> toBeDug = SceneRefs.jobQueue.toBeDug;
        foreach (Rock g in rocks)
        {
            if (!deselect && toBeDug.Contains(g))
            {
                filtered.Remove(g);
            }
            HighLight(deselect ? (Color.red /2) : toBeDugColor , g.gameObject);
        }
        markedTiles = filtered;
    }
    
    /// <summary>
    /// Creates/Deletes pipes to copy the shortest path from startPos to activePos
    /// </summary>
    void CalcPipes()
    {
        Transform pipes = GameObject.FindWithTag("Pipes").transform;
        List<GridPos> path =  PathFinder.FindPath(startPos, activePos, null);
        path.Add(startPos);
        List<ClickableObject> tempMarkedTiles = new();
        List<GridPos> objs = markedTiles.Select(q => new GridPos(q.transform.position)).ToList();
        for (int i = path.Count-1; i >= 0; i--)
        {

            ClickableObject _clickObject = null;
            int index = objs.IndexOf(path[i]);

            if (index > -1)
            {
                _clickObject = markedTiles[index];
                markedTiles.RemoveAt(index);
                objs.RemoveAt(index);
                path.RemoveAt(i);
            }
            else
            {
                _clickObject = Instantiate(buildingPrefab, new(path[i].x, 1.5f, path[i].z), Quaternion.identity, pipes);

                Pipe pipe = _clickObject.GetComponent<Pipe>();
                GridPos gridPos = pipe.GetPos();
                pipe.ChangeRenderMode(true);
                if (!MyGrid.GetGridItem(gridPos, true))
                {
                    HighLight(pipe.CanPlace() ? Color.blue : Color.red, pipe.gameObject);
                    MyGrid.SetGridItem(gridPos, pipe, true);
                    pipe.objectName = pipe.objectName.Replace("(Clone)", " ");
                    pipe.UniqueID();
                }
                else
                {
                    HighLight(Color.red, pipe.gameObject);
                }
            }
            tempMarkedTiles.Add(_clickObject);
        }
        for(int i = markedTiles.Count-1; i >= 0; i--)
        {
            markedTiles[i].GetComponent<Building>().DestoyBuilding();
            markedTiles.RemoveAt(i);
        }
        markedTiles.AddRange(tempMarkedTiles);
        print(markedTiles.Count);
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
            .Where(q => q).Select(q => q.material)
            .Union(toBeChanged.GetComponentsInChildren<SkinnedMeshRenderer>()
            .Where(q => q).Select(q => q.material)))
        {
            LightUp(c, material);
        }
    }

    /// <summary>
    /// Sets highlight to m, or disables it if c is black(0,0,0,N).
    /// </summary>
    /// <param name="c"></param>
    /// <param name="m"></param>
    void LightUp(Color c, Material m)
    {
        if (c.r == 0 && c.g == 0 && c.b == 0)
            m.DisableKeyword("_EMISSION");
        else
        {
            m.SetColor("_EmissionColor", c);
            m.EnableKeyword("_EMISSION");
        }
    }
    #endregion

    #region Control switching
    /// <summary>
    /// Called on press of right mouse button.
    /// </summary>
    public void BreakAction()
    {
        drag = false;
        ChangeSelMode(ControlMode.nothing);
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
            clickedObject = null;
            SceneRefs.infoWindow.Close();
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
                if (buildingPrefab.objectName == buildBlueprint.objectName)
                {
                    ChangeSelMode(ControlMode.nothing);
                }
                else
                {
                    DestroyBlueprint();
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
                    foreach (Rock r in markedTiles)
                    {
                        HighLight(new(), r.gameObject);
                    }
                    markedTiles = new();
                    startPos = null;
                    drag = false;
                    activeControl = ControlMode.nothing;
                    break;
                case ControlMode.build:
                    Camera.main.GetComponent<PhysicsRaycaster>().eventMask = defaultMask;
                    if (buildBlueprint)
                        DestroyBlueprint();
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
                    Camera.main.GetComponent<PhysicsRaycaster>().eventMask = buildingMask;
                    Blueprint();
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
        if (buildBlueprint)
            q = new(buildBlueprint.transform.rotation.x, buildBlueprint.transform.rotation.y, buildBlueprint.transform.rotation.z, buildBlueprint.transform.rotation.w);
        GridPos gp = MyGrid.Rotate(buildingPrefab.blueprint.moveBy, buildingPrefab.transform.eulerAngles.y);
        gp = new(activePos.x + gp.x, (MyGrid.currentLevel * ClickableObjectFactory.LEVEL_HEIGHT) + ClickableObjectFactory.BUILD_OFFSET, activePos.z + gp.z);
        buildBlueprint = Instantiate(buildingPrefab.gameObject, new Vector3(gp.x, gp.y, gp.z), Quaternion.identity, transform).GetComponent<Building>(); // creates the building prefab
        buildBlueprint.transform.rotation = q;
        buildBlueprint.transform.SetParent(buildBlueprint.GetComponent<Pipe>() ? GameObject.FindWithTag("Pipes").transform : GameObject.Find("Buildings").transform);
        buildBlueprint.objectName = buildBlueprint.objectName.Replace("(Clone)", ""); // removes (Clone) from its name
        buildBlueprint.ChangeRenderMode(true);
        HighLight(buildBlueprint.CanPlace() ? Color.blue : Color.red, buildBlueprint.gameObject);
    }

    /// <summary>
    /// Changes the state of rocks in <see cref="markedTiles"/>, if the first one was marked, cancels them.<br/>
    /// Else marks orders their excavation.
    /// </summary>
    void PrepDig()
    {
        List<Rock> toBeDug = SceneRefs.jobQueue.toBeDug;
        drag = false;
        HumanUtil humans = transform.parent.parent.GetChild(2).GetComponent<HumanUtil>();
        if (deselect)
        {
            JobQueue jobQueue = humans.GetComponent<JobQueue>();
            foreach (Rock markTile in markedTiles.Select(q => q.GetComponent<Rock>())) // removes to be dug
            {
                toBeDug.RemoveAll(q => q == markTile);
                markTile.toBeDug = false;
                HighLight(new(), markTile.gameObject);
                jobQueue.CancelJob(JobState.Digging, markTile);
            }
        }
        else
        {
            foreach (var dig in toBeDug) // removes to be dug
            {
                markedTiles.RemoveAll(q => q == dig);
            }
            foreach (Rock tile in markedTiles)
            {
                toBeDug.Add(tile); // add rock
                tile.toBeDug = true;
                humans.GetComponent<JobQueue>().AddJob(JobState.Digging, tile);
            }
            List<Human> workers = humans.transform.GetChild(0).GetComponentsInChildren<Human>().Where(h => h.Job.job == JobState.Free).ToList();
            foreach (var worker in workers) // triggers on every free human
            {
                if (!worker.nightTime)
                {
                    HumanActions.LookForNew(worker);
                }
            }
        }
        markedTiles.Clear();
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
    void DestroyBlueprint()
    {
        MyGrid.GetOverlay().HideGlobalEntryPoints();
        Destroy(buildBlueprint.gameObject);
    }
}