

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
    Upgrade,

    Overlay
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
    public GridTilesMode ActiveControl => controlModes[activeControl];

    [SerializeField] List<GridTilesMode> controlModes;

    public int activeControl = 0;

    public InputAction shiftKey;
        
    public Action DeselectBuildingButton;

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

    public ClickableObject SelectedObject
    {
        get => mouseEvents.selectedObject;
        set => mouseEvents.selectedObject = value;
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
    //public Color ToBeDugColor => multiSelect.ToBeDugColor;

    public Color SelectionColor => ((Nothing)controlModes[(int)ControlMode.Nothing]).SelectionColor;

    public Color DeconstructColor => controlModes[(int)ControlMode.Deconstruct].highlightColor;

    public Color ToBeTempDugColor => controlModes[(int)ControlMode.Dig].highlightColor;
    public Color ToBeDugColor => ((Dig)controlModes[(int)ControlMode.Dig]).ToBeDugColor;
    public Color ToRemoveDugColor => ((Dig)controlModes[(int)ControlMode.Dig]).RemoveColor;


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

        foreach (GridTilesMode mode in controlModes)
        {
            mode.Init(this);
        }
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
            Enter(ActiveObject);
            Drag = false;
        }
    }

    bool inChange = false;
    /// <summary>
    /// Changes the current tool mod, and manages transitions betwean them.
    /// </summary>
    /// <param name="mode"></param>
    public void ChangeSelMode(ControlMode mode)
    {
        if (inChange)
            return;

        inChange = true;
        int i = (int)mode;

        if (i == activeControl)
        {
            if (ActiveControl.ToggleMod())
            {
                inChange = false;
                ChangeSelMode(ControlMode.Nothing);
            }
            else
                DeselectObjects();
        }
        else
        {
            ActiveControl.ExitMod();
            DeselectObjects();
            activeControl = i;



            if (ActiveControl.EnterMod())
                Enter();
        }
        inChange = false;
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