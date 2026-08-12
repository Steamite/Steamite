using Assets.Scripts.UI.Refs___Shortcuts;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.XR.Oculus.Input;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainShortcuts : MonoBehaviour, IAfterLoad, IBeforeLoad
{
    InputActionMap bindingMap => inputAsset.actionMaps[1];

    InputAction dig;
    InputAction upgrade;
    InputAction deconstruction;
    InputAction buildRotate;
    InputAction menu;
    InputAction shift;
    InputAction research;
    InputAction trade;
    InputAction quests;

    [SerializeField] public InputActionAsset inputAsset;
    public static bool handleGrid;
    static bool handleWindows;
    static MainShortcuts instance;

    public Task BeforeInit()
    {
        dig = bindingMap.FindAction("Dig");
        upgrade = bindingMap.FindAction("Upgrade");
        deconstruction = bindingMap.FindAction("Deconstruct");
        buildRotate = bindingMap.FindAction("Build Rotate");
        menu = bindingMap.FindAction("Menu");
        shift = bindingMap.FindAction("Shift");
        research = bindingMap.FindAction("Research");
        trade = bindingMap.FindAction("Trade");
        quests = bindingMap.FindAction("Quests");

        instance = this;
        return Task.CompletedTask;
    }

    public void AfterLoad()
    {
        enabled = true;
        EnableInput();
    }

    public static void DisableInput(bool win = true)
    {
        handleGrid = false;
        handleWindows = win;
        if (win)
            instance.MapWindows();
        else
        {
            instance.ClearActions();
            instance.menu.performed += ShotcutActions.Menu_performed;
        }
        UIRefs.LevelCamera.enabled = false;
        SceneRefs.GridTiles.ActiveObject = null;
    }
    public static void EnableInput()
    {
        instance.ClearActions();
        instance.MapActions();
        handleGrid = true;
        handleWindows = true;
        UIRefs.LevelCamera.enabled = true;
    }

    private void OnEnable()
    {
        bindingMap.Enable();
        MapActions();
    }

    private void OnDisable()
    {
        bindingMap.Disable();
        ClearActions();
    }

    void MapActions()
    {
        dig.performed += ShotcutActions.Dig_performed;
        upgrade.performed += ShotcutActions.Upgrade_performed;
        deconstruction.performed += ShotcutActions.Deconstruction_performed;
        buildRotate.performed += ShotcutActions.BuildRotate_performed;
        shift.performed += ShotcutActions.Shift_performed;
        shift.canceled += ShotcutActions.Shift_canceled;

        research.performed += ShotcutActions.Research_performed;
        trade.performed += ShotcutActions.Trade_performed;
        quests.performed += ShotcutActions.Quests_performed;

        menu.performed += ShotcutActions.Menu_performed;
    }

    void ClearActions()
    {
        dig.performed -= ShotcutActions.Dig_performed;
        upgrade.performed -= ShotcutActions.Upgrade_performed;
        deconstruction.performed -= ShotcutActions.Deconstruction_performed;
        buildRotate.performed -= ShotcutActions.BuildRotate_performed;
        shift.performed -= ShotcutActions.Shift_performed;
        shift.canceled -= ShotcutActions.Shift_canceled;

        research.performed -= ShotcutActions.Research_performed;
        trade.performed -= ShotcutActions.Trade_performed;
        quests.performed -= ShotcutActions.Quests_performed;

        menu.performed -= ShotcutActions.Menu_performed;
    }

    void MapWindows()
    {
        ClearActions();
        research.performed += ShotcutActions.Research_performed;
        trade.performed += ShotcutActions.Trade_performed;
        quests.performed += ShotcutActions.Quests_performed;
        menu.performed += ShotcutActions.Menu_performed;
    }

    public static void DisableAll()
    {
        IAfterLoad[] load = instance.GetComponents<IAfterLoad>();
        foreach (var item in load)
        {
            (item as MonoBehaviour).enabled = false;
        }
        UIRefs.LevelCamera.enabled = false;
    }

    public static void EnableAll()
    {
        IAfterLoad[] load = instance.GetComponents<IAfterLoad>();
        foreach (var item in load)
        {
            (item as MonoBehaviour).enabled = true;
        }
        UIRefs.LevelCamera.enabled = true;
    }

    public static string ParseDescription(string description)
    {
        foreach (var action in instance.bindingMap.actions)
        {
            string newString = action.controls.First().displayName.Replace(":", "");
            description = description.Replace(action.name, $"\"{newString}\"");
        }
        return description;
    }
}