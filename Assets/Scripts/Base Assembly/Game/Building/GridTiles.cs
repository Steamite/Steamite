

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
    Build,
    Upgrade
}
[RequireComponent(typeof(MouseEvents), typeof(MultiSelect), typeof(BuildingActions))]
public class GridTiles : MonoBehaviour
{
    #region Variables
    [SerializeField] MultiSelect multiSelect;
    [SerializeField] MouseEvents mouseEvents;
    [SerializeField] BuildingActions buildingActions;

    //bool buildingPlaced = false;
    /// <summary>Raycast mask for building.</summary>
    public LayerMask buildingMask;
    /// <summary>Default raycast mask for the rest of time.</summary>
    public LayerMask defaultMask;
    public LayerMask pipeMask;

    /// <summary>Current active control mode.</summary>
    public ControlMode ActiveControl { get; private set; } = ControlMode.Nothing;
    public InputAction shiftKey;

    /// <summary>Drag started on a marked tile.</summary>
    public bool deselect = false;
    
    
    public Action DeselectBuildingButton;

    /// <summary>List of all usable cursors.</summary>
    [Tooltip("used to help determine control states")] 
    public Texture2D[] cursors;
    #endregion

    #region Building
    ///<inheritdoc cref="BuildingActions.BlueprintPrefab"/>
    public Building BlueprintPrefab 
    { 
        get => buildingActions.BlueprintPrefab; 
        set => buildingActions.BlueprintPrefab = value; 
    }

    ///<inheritdoc cref="BuildingActions.BlueprintInstance"/>
    public Building BlueprintInstance
    { 
        get => buildingActions.BlueprintInstance; 
        set => buildingActions.BlueprintInstance = value; 
    }

    ///<inheritdoc cref="BuildingActions.Blueprint()"/>
    public void Blueprint() => buildingActions.Blueprint();

    ///<inheritdoc cref="BuildingActions.DestroyBlueprint(bool)"/>
    public void DestroyBlueprint(bool forgetInstance) => buildingActions.DestroyBlueprint(forgetInstance);
    #endregion



    #region Mouse
    ///<inheritdoc cref="MouseEvents.activePos"/>
    public GridPos ActivePos => mouseEvents.activePos;
    ///<inheritdoc cref="MouseEvents.drag"/>
    public bool Drag
    { 
        get => mouseEvents.drag;
        set => mouseEvents.drag = value; 
    }
    ///<inheritdoc cref="MouseEvents.activeObject"/>
    public ClickableObject ActiveObject
    { 
        get => mouseEvents.activeObject; 
        set => mouseEvents.activeObject = value; 
    }

    ///<inheritdoc cref="MouseEvents.Exit()"/>
    public void Exit() => mouseEvents.Exit();

    ///<inheritdoc cref="MouseEvents.Exit(ClickableObject)"/>
    public void Exit(ClickableObject clickableObject) => mouseEvents.Exit(clickableObject);
    


    ///<inheritdoc cref="MouseEvents.Enter()"/>
    public void Enter() => mouseEvents.Enter();

    ///<inheritdoc cref="MouseEvents.Enter(ClickableObject)"/>
    public void Enter(ClickableObject clickableObject) => mouseEvents.Enter(clickableObject);


    ///<inheritdoc cref="MouseEvents.Down"/>
    public void Down() => mouseEvents.Down();
    ///<inheritdoc cref="MouseEvents.Up()"/>
    public void Up() => mouseEvents.Up();

    ///<inheritdoc cref="MouseEvents.DeselectObjects"/>
    public void DeselectObjects() => mouseEvents.DeselectObjects();
    ///<inheritdoc cref="MouseEvents.Clear"/>
    public void Clear() => mouseEvents.Clear();
    #endregion



    #region MultiSelect
    ///<inheritdoc cref="MultiSelect.ToBeDugColor"/>
    public Color ToBeDugColor => multiSelect.ToBeDugColor;

    ///<inheritdoc cref="MultiSelect.InitPipes(GridPos, Pipe)"/>
    public void InitPipes(GridPos pos, Pipe pipe) => multiSelect.InitPipes(pos, pipe);
    
    ///<inheritdoc cref="MultiSelect.CalcPipes(GridPos, Pipe)"/>
    public void CalcPipes(GridPos pos, Pipe pipe) => multiSelect.CalcPipes(pos, pipe);
    
    ///<inheritdoc cref="MultiSelect.ClickPipes(GridPos)"/>
    public bool ClickPipes(GridPos pos) => multiSelect.ClickPipes(pos);
    
    ///<inheritdoc cref="MultiSelect.ClearPipes"/>
    public void ClearPipes() => multiSelect.ClearPipes();

    ///<inheritdoc cref="MultiSelect.InitDig(Rock)"/>
    public void InitDig(Rock rock) => multiSelect.InitDig(rock);
    ///<inheritdoc cref="MultiSelect.DigMark(Rock)"/>
    public void DigMark(Rock rock) => multiSelect.DigMark(rock);

    ///<inheritdoc cref="MultiSelect.ClearDig()"/>
    public void ClearDig() => multiSelect.ClearDig();

    ///<inheritdoc cref="MultiSelect.CalcTiles(GridPos)"/>
    public void CalcTiles(GridPos pos) => multiSelect.CalcTiles(pos);
    ///<inheritdoc cref="MultiSelect.RemoveFromMarked(ClickableObject)"/>
    public void RemoveFromMarked(ClickableObject cO) => multiSelect.RemoveFromMarked(cO);
    ///<inheritdoc cref="MultiSelect.Break()"/>
    public void Break() => multiSelect.Break();
    #endregion


    private void Awake()
    {
        multiSelect = GetComponent<MultiSelect>();
        mouseEvents = GetComponent<MouseEvents>();
        buildingActions = GetComponent<BuildingActions>();
    }

    /// <summary>Called when AltTabing from the game.</summary>
    void OnApplicationFocus(bool focus)
    {
        if (!focus)
        {
            Clear();
        }
    }

    #region Control switching
    /// <summary>
    /// Called on press of right mouse button.
    /// </summary>
    public void BreakAction()
    {
        if (multiSelect.Break())
        {
            ChangeSelMode(ControlMode.Nothing);
            Drag = false;
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
                if (BlueprintPrefab.Name == BlueprintInstance.Name)
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
                    ClearDig();
                    Drag = false;
                    ActiveControl = ControlMode.Nothing;
                    break;
                case ControlMode.Upgrade:
                    ActiveControl = ControlMode.Nothing;
                    break;
                case ControlMode.Build:
                    SceneRefs.CameraSceneMover.SetRaycastMask(defaultMask);
                    if (Drag)
                    {
                        ClearPipes();
                        DeselectBuildingButton?.Invoke();
                        Drag = false;
                    }
                    else if (BlueprintInstance)
                        DestroyBlueprint(true);
                    shiftKey.Disable();
                    break;
            }
            DeselectObjects();
            EnterMode(mode);
        }
    }

    void EnterMode(ControlMode mode)
    {
        bool visible = true;
        Texture2D cur = null;
        Vector2 vec = new();
        ActiveControl = mode;
        switch (mode)
        {
            case ControlMode.Nothing:
                cur = default;
                vec = Vector2.zero;
                break;
            case ControlMode.Deconstruct:
                cur = cursors[0];
                vec = new(15, 15);
                break;
            case ControlMode.Dig:
                cur = cursors[1];
                vec = new(1, 16);
                break;
            case ControlMode.Upgrade:
                cur = cursors[2];
                vec = new(15, 1);
                break;
            case ControlMode.Build:
                cur = default;
                vec = Vector2.zero;
                if (BlueprintPrefab is Pipe)
                    SceneRefs.CameraSceneMover.SetRaycastMask(pipeMask);
                else
                    SceneRefs.CameraSceneMover.SetRaycastMask(buildingMask);
                Blueprint();
                shiftKey.Enable();
                return;
        }
        Enter();


        if (visible)
        {
            Cursor.SetCursor(cur, vec, CursorMode.Auto);
        }
    }
    #endregion



    /// <summary>
    /// If the object was selected unselect.(Happens when the object is destroyed)
    /// </summary>
    /// <param name="cO"></param>
    public void DestroyUnselect(ClickableObject cO)
    {
        RemoveFromMarked(cO);
        Exit(cO);
        if (ActiveObject && ActiveObject == cO)
            ActiveObject = null;
        if (cO.selected)
        {
            DeselectObjects();
        }
    }

    
}