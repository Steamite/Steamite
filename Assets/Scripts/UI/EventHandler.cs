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



    [SerializeField] public InputActionAsset inputAsset;
    [Header("Menus")]
    [SerializeField] GameObject buildMenuObject;
    [SerializeField] Transform catogoryMenu;

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
        GridTiles gt = transform.GetChild(0).GetComponent<GridTiles>();
        // toggle build menu
        if (buildMenu.triggered)
        {
            buildMenuObject.SetActive(!buildMenuObject.activeSelf);
            for (int i = catogoryMenu.childCount - 1; i <= 0; i++)
            {
                catogoryMenu.GetChild(i).gameObject.SetActive(buildMenuObject.activeSelf);
            }
            buildMenuObject.transform.GetChild(2).gameObject.SetActive(false);
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
            GameObject menu = GameObject.Find("Ingame Menu").transform.GetChild(0).gameObject;
            if (menu.activeSelf)
            {
                GameObject.Find("Scene").GetComponent<Tick>().Unpause();
            }
            else
            {
                GameObject.Find("Scene").GetComponent<Tick>().ChangeGameSpeed(0);
            }
            menu.SetActive(!menu.activeSelf);
            //menu.transform.parent.GetChild(1).gameObject.SetActive(menu.activeSelf);
            Camera.main.GetComponent<PhysicsRaycaster>().enabled = !menu.activeSelf;
            Camera.main.GetComponent<Physics2DRaycaster>().enabled = !menu.activeSelf;
        }

        if (shift.inProgress)
        {
            if(gt.selMode == SelectionMode.deconstruct)
            {
                gt.Enter(gt.activeObject);
            }
        }
    }
}
