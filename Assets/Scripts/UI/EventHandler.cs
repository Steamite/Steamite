using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class EventHandler : MonoBehaviour
{
    InputActionMap bindingMap => inputAsset.actionMaps[1];

    InputAction buildMenu => bindingMap.FindAction("Build Menu");
    InputAction dig => bindingMap.FindAction("Dig");
    InputAction deconstruction => bindingMap.FindAction("Deconstruct");
    InputAction buildRotate => bindingMap.FindAction("Build Rotate");
    InputAction menu => bindingMap.FindAction("Menu");
    InputAction shift => bindingMap.FindAction("Shift");
    InputAction research => bindingMap.FindAction("Research");


    [SerializeField] public InputActionAsset inputAsset;

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
        GridTiles gt = MyGrid.gridTiles;
        // toggle build menu
        if (buildMenu.triggered)
        {
            GameObject buildMenu = MyGrid.canvasManager.buildMenu.gameObject;
            Transform categories = buildMenu.transform.GetChild(1);
            buildMenu.SetActive(!buildMenu.activeSelf);
            if (buildMenu.activeSelf)
            {
                for (int i = 0; i < categories.childCount; i++)
                {
                    categories.GetChild(i).gameObject.SetActive(false);
                }
                buildMenu.transform.GetChild(1).gameObject.SetActive(false);
            }
        }
        // toggle dig
        if (dig.triggered)
        {
            gt.ChangeSelMode(SelectionMode.dig);
            gt.Exit(gt.activeObject);
            gt.Enter(gt.activeObject);
        }
        // toggle deconstruct
        else if (deconstruction.triggered)
        {
            gt.ChangeSelMode(SelectionMode.deconstruct);
            gt.Exit(gt.activeObject);
            gt.Enter(gt.activeObject);
        }
        // rotates buildign
        else if (buildRotate.triggered)
        {
            float axis = buildRotate.ReadValue<float>();
            if(MyGrid.gridTiles.selMode == SelectionMode.build)
            {
                if (MyGrid.gridTiles.buildBlueprint.GetComponent<Pipe>())
                    return;
                if (axis < 0)
                {
                    MyGrid.gridTiles.buildBlueprint.transform.Rotate(new Vector3(0, 90, 0));
                }
                else
                {
                    MyGrid.gridTiles.buildBlueprint.transform.Rotate(new Vector3(0, -90, 0));
                }
                MyGrid.gridTiles.Enter(MyGrid.gridTiles.activeObject);
            }
        }
        // opens ingame menu
        if (menu.triggered)
        {
            MyGrid.canvasManager.pauseMenu.Toggle();
        }

        if (shift.inProgress)
        {
            if(gt.selMode == SelectionMode.deconstruct)
            {
                gt.Enter(gt.activeObject);
            }
        }

        if (research.triggered)
        {
            MyGrid.canvasManager.research.ToogleResearchUI();
        }
    }
}
