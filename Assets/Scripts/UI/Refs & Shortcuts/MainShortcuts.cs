using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainShortcuts : MonoBehaviour, IAfterLoad, IBeforeLoad
{
    InputActionMap bindingMap => inputAsset.actionMaps[1];

    InputAction buildMenu;
    InputAction dig;
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
        buildMenu = bindingMap.FindAction("Build Menu");
        dig = bindingMap.FindAction("Dig");
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

    public void AfterInit()
    {
        enabled = true;
        EnableInput();
    }

    public static void DisableInput(bool win = true)
    {
        handleGrid = false;
        handleWindows = win;
        UIRefs.LevelCamera.enabled = false;
        SceneRefs.GridTiles.activeObject = null;
    }
    public static void EnableInput()
    {
        handleGrid = true;
        handleWindows = true;
        UIRefs.LevelCamera.enabled = true;
    }

    private void OnEnable()
    {
        bindingMap.Enable();
    }
    private void OnDisable()
    {
        bindingMap.Disable();
    }

    void Update()
    {
        GridTiles gt = SceneRefs.GridTiles;
        if (handleGrid)
        {
            // toggle build menu
            if (buildMenu.triggered)
            {
                // buildMenu = UIRefs.buildBar;
                /*Transform categories = buildMenu.GetChild(1);
                buildMenu.gameObject.SetActive(!buildMenu.gameObject.activeSelf);
                if (buildMenu.gameObject.activeSelf)
                {
                    for (int i = 0; i < categories.childCount; i++)
                    {
                        categories.GetChild(i).gameObject.SetActive(false);
                    }
                    buildMenu.GetChild(1).gameObject.SetActive(false);
                }*/
            }
            // toggle dig
            if (dig.triggered)
            {
                gt.ChangeSelMode(ControlMode.Dig);
            }
            // toggle deconstruct
            else if (deconstruction.triggered)
            {
                gt.ChangeSelMode(ControlMode.Deconstruct);
            }
            // rotates building
            else if (buildRotate.triggered)
            {
                float axis = buildRotate.ReadValue<float>();
                if (SceneRefs.GridTiles.ActiveControl == ControlMode.Build)
                {
                    Building building = SceneRefs.GridTiles.BlueprintInstance;
                    if (building is Pipe)
                        return;
                    if (axis < 0)
                    {
                        building.transform.Rotate(new Vector3(0, 90, 0));
                    }
                    else
                    {
                        building.transform.Rotate(new Vector3(0, -90, 0));
                    }
                    if (building is IFluidWork fluid)
                    {
                        fluid.AttachedPipes.ForEach(q => q.RecalculatePipeTransform());
                    }
                    SceneRefs.GridTiles.Enter(SceneRefs.GridTiles.activeObject);
                }
            }
        }

        if (shift.inProgress)
        {
            if (gt.ActiveControl == ControlMode.Deconstruct)
            {
                gt.Enter(gt.activeObject);
            }
        }

        if (handleWindows)
        {
            if (research.triggered)
            {
                UIRefs.ResearchWindow.ToggleWindow();
            }
            else if (trade.triggered)
            {
                UIRefs.TradingWindow.ToggleWindow();
            }
            else if (quests.triggered)
            {
                UIRefs.Quests.ToggleWindow();
            }
        }
        // opens ingame menu
        if (menu.triggered)
        {
            UIRefs.PauseMenu.Toggle();
        }
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
    }

    public static string ParseDescription(string description)
    {
        foreach (var action in instance.GetActions())
        {
            string newString = action.controls.First().displayName.Replace(":", "");
            description = description.Replace(action.name, $"\"{newString}\"");
        }
        return description;
    }

    List<InputAction> GetActions()
    {
        List<InputAction> inputActions = new()
        {
            buildMenu,
            dig,
            deconstruction,
            buildRotate,
            menu,
            shift,
            research,
            trade,
            quests,
        };
        return inputActions;
    }
}