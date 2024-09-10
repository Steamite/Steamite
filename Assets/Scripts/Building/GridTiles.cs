using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public enum SelectionMode
{
    nothing,
    deconstruct,
    dig,
    build
}

public class GridTiles : MonoBehaviour
{
    //public bool dragBuildState = false; // follow mouse or find closest route
    public LayerMask buildingMask;
    public LayerMask defaultMask;
    [Header("Tilemaps")]
    public Building buildingPrefab;
    public SelectionMode selMode = SelectionMode.nothing;
    public GridPos activePos;
    public GridPos startPos;
    public bool drag = false;
    public bool deselect = false;
    public List<ClickableObject> markedTiles;
    public List<Rock> toBeDigged = new();
    public Building buildBlueprint;
    /// <summary> clicked(selected) </summary>
    public ClickableObject clickedObject;
    /// <summary> mouse over </summary>
    public ClickableObject activeObject;
    public Texture2D[] cursors;
    public Color highlight = Color.white/3; // WHITE / 3
    public Color toBeDugColor = (Color.yellow + Color.red) / 2;
    /// <summary>
    /// Called when altTabing from the game.
    /// </summary>
    void OnApplicationFocus(bool focus)
    {
        if (!focus)
        {
            Exit(activeObject);
            activeObject = null;
        }
    }

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
        activePos = new(enterObject.transform.position);
        switch (selMode)
        {
            case SelectionMode.nothing:
                
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
                        if (b.build.deconstructing)
                            c += Color.red / 2;/*
                        else if (!b.build.constructed)
                            c +=;*/
                    }
                }
                break;
            case SelectionMode.deconstruct:
                Building _b = enterObject.GetComponent<Building>();
                if (_b)
                    if (_b.build.deconstructing)
                        c = Color.red / 2;
                    else
                        c = Color.red;
                else
                    return;
                break;
            case SelectionMode.dig:
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
            case SelectionMode.build:
                if (drag)
                {
                    CalcPipes();
                    return;
                }
                else
                {
                    Build __b = buildBlueprint.build;
                    GridPos grid = MyGrid.CheckRotation(__b.blueprint.moveBy, buildBlueprint.transform.eulerAngles.y);
                    grid = new(activePos.x + grid.x, buildBlueprint.transform.position.y, activePos.z + grid.z);
                    buildBlueprint.transform.position = new(grid.x, grid.level, grid.z);
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
        switch (selMode)
        {
            case SelectionMode.nothing:
                Rock r = exitObject.GetComponent<Rock>();
                Building b = exitObject.GetComponent<Building>();
                Pipe pipe = exitObject.GetComponent<Pipe>();
                if (exitObject.selected)
                    c = highlight * 2;
                else
                    c = new();
                if (r && r.toBeDug)
                    c += toBeDugColor;
                else if (b && b.build.deconstructing)
                    c += Color.red/2;
                else if (pipe)
                {
                    HighLight(c, pipe.gameObject);
                    return;
                }

                break;
            case SelectionMode.deconstruct:
                Building _b = exitObject.GetComponent<Building>();
                if (_b && _b.build.deconstructing)
                    c = Color.red * 0.75f;
                else
                    c = new();
                break;
            case SelectionMode.dig:
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
            case SelectionMode.build:
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
        switch (selMode)
        {
            case SelectionMode.nothing:
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
                clickedObject.OpenWindow(true);
                break;
            case SelectionMode.deconstruct:
                Building b = activeObject.GetComponent<Building>();
                if (b)
                {
                    b.OrderDeconstruct();
                    if (b && !b.build.deconstructing)
                        c = Color.red;
                    else
                        c = Color.red / 2;
                    HighLight(c, b.gameObject);
                }
                break;
            case SelectionMode.dig:
                Rock _r = activeObject.GetComponent<Rock>();
                if (_r)
                {
                    startPos = new(activePos.x, activePos.z);
                    drag = true;
                    deselect = _r.toBeDug ? true : false;
                    markedTiles = new() { _r };
                }
                break;
            case SelectionMode.build:
                if (buildBlueprint.CanPlace())
                {
                    if (buildBlueprint.GetComponent<Pipe>())
                    {
                        drag = true;
                        markedTiles = new() { buildBlueprint};
                        GridPos gridPos = new(activeObject.gameObject);
                        MyGrid.pipeGrid[(int)gridPos.x, (int)gridPos.z] = buildBlueprint.GetComponent<Pipe>();
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
        switch (selMode)
        {
            case SelectionMode.nothing:
            case SelectionMode.deconstruct:
                // nothing
                break;
            case SelectionMode.build:

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
                        ChangeSelMode(SelectionMode.nothing);
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
                        ChangeSelMode(SelectionMode.nothing);
                    }
                }
                else 
                {
                    Debug.LogWarning("Can't place here!!");
                }
                break;
            case SelectionMode.dig:
                PrepDig();
                markedTiles = new();
                break;
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
        rocks.AddRange(Physics.OverlapBox(new Vector3(startPos.x - x, 0.5f, startPos.z - z), new(Mathf.Abs(x), 0.5f, Mathf.Abs(z))).Where(q => q.GetComponent<Rock>() != null).Select(q => q.GetComponent<Rock>()).ToList());
        List<ClickableObject> filtered = rocks.ToList();
        foreach (Rock g in rocks)
        {
            if (!deselect && toBeDigged.Contains(g))
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
                GridPos gridPos = new(pipe.gameObject);
                pipe.ChangeRenderMode(true);
                if (!MyGrid.pipeGrid[(int)gridPos.x, (int)gridPos.z])
                {
                    HighLight(pipe.CanPlace() ? Color.blue : Color.red, pipe.gameObject);
                    MyGrid.pipeGrid[(int)gridPos.x, (int)gridPos.z] = pipe;
                    pipe.name = pipe.name.Replace("(Clone)", " ");
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

    /// <summary>
    /// Called when canceling drag, changes highlight of all rocks in markedTiles.
    /// </summary>
    public void ClearMarks() // deletes all tiles
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
    /// HighLights all materials on toBeChanged to c.
    /// </summary>
    /// <param name="c"></param>
    /// <param name="toBeChanged"></param>
    public void HighLight(Color c, GameObject toBeChanged)
    {
        foreach (Material material in toBeChanged.GetComponentsInChildren<MeshRenderer>().Where(q=> q).Select(q=> q.material))
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

    /// <summary>
    /// Called on press of right mouse button.
    /// </summary>
    public void BreakAction()
    {
        drag = false;
        ChangeSelMode(SelectionMode.nothing);
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
            GameObject.FindWithTag("Info").transform.GetChild(0).gameObject.SetActive(false);
        }
    }
    /// <summary>
    /// Changes the current tool mod, and manages transitions betwean them.
    /// </summary>
    /// <param name="mode"></param>
    public void ChangeSelMode(SelectionMode mode)
    {
        if(mode == selMode && mode != SelectionMode.nothing)
        {
            if(selMode == SelectionMode.build)
            {
                if(buildingPrefab.name == buildBlueprint.name)
                {
                    ChangeSelMode(SelectionMode.nothing);
                }
                else
                {
                    MyGrid.sceneReferences.overlay.DeleteBuildGrid();
                    buildBlueprint.DestoyBuilding();
                    Blueprint();
                    return;
                }
            }
            ChangeSelMode(SelectionMode.nothing);
        }
        else
        {
            switch (selMode)
            {
                case SelectionMode.deconstruct:
                    selMode = SelectionMode.nothing;
                    break;
                case SelectionMode.dig:
                    foreach(Rock r in markedTiles)
                    {
                        HighLight(new(), r.gameObject);
                    }
                    markedTiles = new();
                    startPos = null;
                    drag = false;
                    selMode = SelectionMode.nothing;
                    break;
                case SelectionMode.build:
                    Camera.main.GetComponent<PhysicsRaycaster>().eventMask = defaultMask;
                    MyGrid.sceneReferences.overlay.DeleteBuildGrid();
                    if (buildBlueprint)
                        buildBlueprint.DestoyBuilding();
                    foreach(ClickableObject clickable in markedTiles)
                    {
                        clickable.GetComponent<Building>().DestoyBuilding();
                    }
                    markedTiles = new();
                    break;
            }
            DeselectObjects();
            bool visible = true;
            Texture2D cur = null;
            Vector2 vec = new();
            selMode = mode;
            switch (mode)
            {
                case SelectionMode.nothing:
                    cur = default;
                    vec = Vector2.zero;
                    Enter(activeObject);
                    break;
                case SelectionMode.deconstruct:
                    cur = cursors[0];
                    vec = new(15f, 15f);
                    break;
                case SelectionMode.dig:
                    cur = cursors[1];
                    vec = new(1, 16f);
                    break;
                case SelectionMode.build:
                    cur = default;
                    vec = Vector2.zero;
                    Camera.main.GetComponent<PhysicsRaycaster>().eventMask = buildingMask;
                    Blueprint();
                    break;
            }
            if (visible)
            {
                Cursor.SetCursor(cur, vec, CursorMode.ForceSoftware);
            }
        }   
    }

    /// <summary>
    /// Instantiates and sets a copy of a building prefab.
    /// </summary>
    void Blueprint()
    {
        Quaternion q = new();
        if (buildBlueprint)
            q = new(buildBlueprint.transform.rotation.x, buildBlueprint.transform.rotation.y, buildBlueprint.transform.rotation.z, buildBlueprint.transform.rotation.w);
        GridPos gp = MyGrid.CheckRotation(buildingPrefab.build.blueprint.moveBy, buildingPrefab.transform.eulerAngles.y);
        gp = new(activePos.x + gp.x, buildingPrefab is Pipe ? 0.75f : 0.5f, activePos.z + gp.z);
        buildBlueprint = Instantiate(buildingPrefab.gameObject, gp.ToVec(), Quaternion.identity, transform).GetComponent<Building>(); // creates the building prefab
        buildBlueprint.transform.rotation = q;
        buildBlueprint.transform.SetParent(buildBlueprint.GetComponent<Pipe>() ? GameObject.FindWithTag("Pipes").transform : GameObject.Find("Buildings").transform);
        buildBlueprint.name = buildBlueprint.name.Replace("(Clone)", ""); // removes (Clone) from its name
        buildBlueprint.ChangeRenderMode(true);
        HighLight(buildBlueprint.CanPlace() ? Color.blue : Color.red, buildBlueprint.gameObject);
    }

    /// <summary>
    /// Changes the state of rocks in markedTiles, if the first one was toBeDigged, cancels them. Else marks orders their excavation.
    /// </summary>
    void PrepDig()
    {
        drag = false;
        Humans humans = transform.parent.parent.GetChild(2).GetComponent<Humans>();
        if (deselect)
        {
            JobQueue jobQueue = humans.GetComponent<JobQueue>();
            foreach (Rock markTile in markedTiles.Select(q => q.GetComponent<Rock>())) // removes to be dug
            {
                toBeDigged.RemoveAll(q => q == markTile);
                markTile.toBeDug = false;
                HighLight(new(), markTile.gameObject);
                jobQueue.CancelJob(JobState.Digging, markTile);
            }
        }
        else
        {
            foreach (var toBeDug in toBeDigged) // removes to be dug
            {
                markedTiles.RemoveAll(q => q == toBeDug);
            }
            foreach (Rock tile in markedTiles)
            {
                toBeDigged.Add(tile); // add rock
                tile.toBeDug = true;
                humans.GetComponent<JobQueue>().AddJob(JobState.Digging, tile);
            }
            List<Human> workers = humans.transform.GetChild(0).GetComponentsInChildren<Human>().Where(h => h.jData.job == JobState.Free).ToList();
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
    /// If the object was selected unselect.
    /// </summary>
    /// <param name="cO"></param>
    public void Remove(ClickableObject cO)
    {
        Exit(cO);
        if (activeObject && activeObject == cO)
            activeObject = null;
        if (cO.selected)
        {
            GameObject.FindWithTag("Info").transform.GetChild(0).gameObject.SetActive(false);
            clickedObject = null;
        }
    }
}