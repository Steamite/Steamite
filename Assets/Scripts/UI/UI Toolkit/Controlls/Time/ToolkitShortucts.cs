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
    
	public void Init(VisualElement bottomBar)
	{
		shift = inputAsset.actionMaps[1].FindAction("Shift");

		gameSpeed = smallShortcuts.FindAction("Game Speed");
		level = smallShortcuts.FindAction("Level");

		timeButtons = bottomBar.Q<TimeButtons>();
        timeButtons.Start();
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
                //levelButtons.OutsideTrigger(Mathf.RoundToInt(level.ReadValue<float>()));
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
