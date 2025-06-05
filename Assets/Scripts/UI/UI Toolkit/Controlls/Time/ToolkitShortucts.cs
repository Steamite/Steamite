using BottomBar.Building;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class ToolkitShortucts : MonoBehaviour, IToolkitController
{
    [SerializeField] public InputActionAsset inputAsset;
    public TimeButtons timeButtons;
    //[SerializeField] public RadioButtons levelButtons;

    InputAction shift;

    InputActionMap smallShortcuts => inputAsset.actionMaps[2];
    InputAction gameSpeed;
    InputAction level;
    InputAction buildMenu;

    public void Init(VisualElement bottomBar)
    {
        shift = inputAsset.actionMaps[1].FindAction("Shift");

        gameSpeed = smallShortcuts.FindAction("Game Speed");
        level = smallShortcuts.FindAction("Level");
        buildMenu = smallShortcuts.FindAction("Build Menu");

        timeButtons = bottomBar.Q<TimeButtons>();
        timeButtons.Start();
        enabled = true;
    }

    private void OnEnable()
    {
        smallShortcuts.Enable();
    }
    private void OnDisable()
    {
        smallShortcuts.Disable();
    }

    private void Update()
    {
        if (MainShortcuts.handleGrid)
        {
            if (shift.inProgress)
            {
                if (level.triggered)
                {
                    //levelButtons.OutsideTrigger(Mathf.RoundToInt(level.ReadValue<float>()));
                }
            }
            else
            {
                if (gameSpeed.triggered)
                {
                    int i = Mathf.RoundToInt(gameSpeed.ReadValue<float>());
                    Debug.Log("New: " + i);
                    timeButtons.OutsideTrigger(i);
                }
            }


            if (buildMenu.triggered)
            {
                ((BuildMenu)UIRefs.bottomBar[0]).Toggle();
            }
        }
    }

}
