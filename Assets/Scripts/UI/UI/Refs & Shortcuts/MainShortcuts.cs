using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class MainShortcuts : MonoBehaviour
{
    InputActionMap bindingMap => inputAsset.actionMaps[1];

    InputAction buildMenu => bindingMap.FindAction("Build Menu");
    InputAction dig => bindingMap.FindAction("Dig");
    InputAction deconstruction => bindingMap.FindAction("Deconstruct");
    InputAction buildRotate => bindingMap.FindAction("Build Rotate");
    InputAction menu => bindingMap.FindAction("Menu");
    InputAction shift => bindingMap.FindAction("Shift");
    InputAction research => bindingMap.FindAction("Research");
    InputAction trade => bindingMap.FindAction("Trade");

    [SerializeField] public InputActionAsset inputAsset;
    static bool handleGrid = true;
    static bool handleWindows = true;

    public static void DisableInput(bool win = true)
    {
        handleGrid = false;
        handleWindows = win;
        UIRefs.levelCamera.enabled = false;
        SceneRefs.gridTiles.activeObject = null;
    }
    public static void EnableInput()
    {
        handleGrid = true;
        handleWindows = true;
        UIRefs.levelCamera.enabled = true;
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
        GridTiles gt = SceneRefs.gridTiles;
        if (handleGrid)
        {
            // toggle build menu
            if (buildMenu.triggered)
            {
                Transform buildMenu = SceneRefs.buildMenu.transform;
                Transform categories = buildMenu.GetChild(1);
                buildMenu.gameObject.SetActive(!buildMenu.gameObject.activeSelf);
                if (buildMenu.gameObject.activeSelf)
                {
                    for (int i = 0; i < categories.childCount; i++)
                    {
                        categories.GetChild(i).gameObject.SetActive(false);
                    }
                    buildMenu.GetChild(1).gameObject.SetActive(false);
                }
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
            // rotates buildign
            else if (buildRotate.triggered)
            {
                float axis = buildRotate.ReadValue<float>();
                if (SceneRefs.gridTiles.activeControl == ControlMode.build)
                {
                    if (SceneRefs.gridTiles.buildBlueprint.GetComponent<Pipe>())
                        return;
                    if (axis < 0)
                    {
                        SceneRefs.gridTiles.buildBlueprint.transform.Rotate(new Vector3(0, 90, 0));
                    }
                    else
                    {
                        SceneRefs.gridTiles.buildBlueprint.transform.Rotate(new Vector3(0, -90, 0));
                    }
                    SceneRefs.gridTiles.Enter(SceneRefs.gridTiles.activeObject);
                }
            }
        }

        if (shift.inProgress)
        {
            if(gt.activeControl == ControlMode.deconstruct)
            {
                gt.Enter(gt.activeObject);
            }
        }

        if (handleWindows)
        {
            if (research.triggered)
            {
                if(UIRefs.trading.window.style.display == DisplayStyle.Flex)
                    UIRefs.trading.CloseWindow();
                UIRefs.research.ToggleWindow();
            }
            if (trade.triggered)
            {
				/*if(UIRefs.research.window.style.display == DisplayStyle.Flex)
                    UIRefs.research.CloseWindow();*/
				UIRefs.trading.ToggleWindow();
			}
        }
        // opens ingame menu
        if (menu.triggered)
        {
            UIRefs.pauseMenu.Toggle();
        }
    }
}
