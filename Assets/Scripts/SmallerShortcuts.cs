using UnityEngine;
using UnityEngine.InputSystem;

public class SmallerShortcuts : MonoBehaviour
{
    [SerializeField] public InputActionAsset inputAsset;
    [SerializeField] public RadioButtons timeButtons;
    [SerializeField] public RadioButtons levelButtons;
    InputAction shift => inputAsset.actionMaps[1].FindAction("Shift");

    InputActionMap bindingMap => inputAsset.actionMaps[2];
    InputAction gameSpeed => bindingMap.FindAction("Game Speed");
    InputAction level => bindingMap.FindAction("Level");
    
    static bool handleGrid = true;
    static bool handleWindows = false;

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
        bindingMap.Enable();
    }
    private void OnDisable()
    {
        bindingMap.Disable();
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
