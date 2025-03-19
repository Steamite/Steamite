using UnityEngine;
using UnityEngine.InputSystem;

public class SmallerShortcuts : MonoBehaviour
{
    [SerializeField] public InputActionAsset inputAsset;
    [SerializeField] public RadioButtons timeButtons;
    [SerializeField] public RadioButtons levelButtons;

    InputAction shift;

    InputActionMap smallShortcuts;
    InputAction gameSpeed;
    InputAction level;

    static bool handleGrid = true;
    static bool handleWindows = false;

    private void Awake()
    {
        shift = inputAsset.actionMaps[1].FindAction("Shift");

        smallShortcuts = inputAsset.actionMaps[2];
        gameSpeed = smallShortcuts.FindAction("Game Speed");
        level = smallShortcuts.FindAction("Level");
    }

    public static void DisableInput(bool win = true)
    {
        handleGrid = false;
        handleWindows = win;
    }
    public static void EnableInput()
    {
        handleGrid = true;
        handleWindows = false;
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
        if (shift.inProgress)
        {
            if (level.triggered)
            {
                levelButtons.OutsideTrigger(Mathf.RoundToInt(level.ReadValue<float>()));
            }
        }
        else
        {
            if (gameSpeed.triggered)
            {
                timeButtons.OutsideTrigger(Mathf.RoundToInt(gameSpeed.ReadValue<float>()));
            }
        }
    }
}
