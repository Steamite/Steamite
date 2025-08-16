using UnityEngine;
using UnityEngine.InputSystem;

public class MainShortcuts : MonoBehaviour, IAfterLoad
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

    public void Init()
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
                gt.ChangeSelMode(ControlMode.dig);
                gt.Exit(gt.activeObject);
                gt.Enter(gt.activeObject);
            }
            // toggle deconstruct
            else if (deconstruction.triggered)
            {
                gt.ChangeSelMode(ControlMode.deconstruct);
                gt.Exit(gt.activeObject);
                gt.Enter(gt.activeObject);
            }
            // rotates building
            else if (buildRotate.triggered)
            {
                float axis = buildRotate.ReadValue<float>();
                if (SceneRefs.GridTiles.activeControl == ControlMode.build)
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
                    if(building is IFluidWork fluid)
                    {
                        fluid.AttachedPipes.ForEach(q => q.RecalculatePipeTransform());
                    }
                    SceneRefs.GridTiles.Enter(SceneRefs.GridTiles.activeObject);
                }
            }
        }

        if (shift.inProgress)
        {
            if (gt.activeControl == ControlMode.deconstruct)
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
}
