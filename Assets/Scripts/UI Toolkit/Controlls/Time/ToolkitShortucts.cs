using BottomBar.Building;
using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class ToolkitShortucts : MonoBehaviour, IAfterLoad
{
    [SerializeField] InputActionAsset inputAsset;
    TimeButtons timeButtons;
    LevelButtons levelButtons;
    OverlayButtons overlayButtons;
    //[SerializeField] public RadioButtons levelButtons;

    InputAction shift;

    InputActionMap smallShortcuts => inputAsset.actionMaps[2];
    InputAction gameSpeed;
    InputAction level;
    InputAction buildMenu;

    public void AfterLoad()
    {
        VisualElement topBar = UIRefs.TopBarRoot;
        shift = inputAsset.actionMaps[1].FindAction("Shift");

        gameSpeed = smallShortcuts.FindAction("Game Speed");
        level = smallShortcuts.FindAction("Level");
        buildMenu = smallShortcuts.FindAction("Build Menu");

        UIRefs.TopBar.RegisterReload(ReloadUI);
        enabled = true;
    }

    private void ReloadUI(PanelRenderer panelRenderer, VisualElement rootElement)
    {
        timeButtons = rootElement.Q<TimeButtons>();
        timeButtons.Init();

        levelButtons = rootElement.Q<LevelButtons>();
        levelButtons.Init();

        overlayButtons = rootElement.Q<OverlayButtons>();
        overlayButtons.Init();

        ProgressBar trustBar = rootElement.Q<ProgressBar>("Trust");
        trustBar.SetBinding(
            nameof(QuestController.Trust),
            nameof(ProgressBar.value),
            (ref int i) =>
            {
                trustBar.title = $"{i}/100";
                //trustBar[0][0][0].style.color
                return (float)i;
            },
            SceneRefs.QuestController);
    }

    private void OnEnable()
    {
        smallShortcuts.Enable();

        gameSpeed.performed += GameSpeed_performed;
        level.performed += Level_performed;
        buildMenu.performed += BuildMenu_performed;
    }
    private void OnDisable()
    {
        smallShortcuts.Disable();

        gameSpeed.performed -= GameSpeed_performed;
        level.performed -= Level_performed;
        buildMenu.performed -= BuildMenu_performed;
    }

    bool CanActivate => MainShortcuts.handleGrid;

    void BuildMenu_performed(InputAction.CallbackContext obj)
    {
        if (!CanActivate)
            return;
        UIRefs.BottomBarRoot.Q<BuildMenu>().Toggle();
    }

    void Level_performed(InputAction.CallbackContext obj)
    {
        if (!CanActivate)
            return;

        int i = Mathf.RoundToInt(level.ReadValue<float>());
        MyGrid.ChangeGridLevel(i);
    }

    void GameSpeed_performed(InputAction.CallbackContext obj)
    {
        if (!CanActivate)
            return;

        int i = Mathf.RoundToInt(obj.ReadValue<float>());
        SceneRefs.Tick.ChangeGameSpeed(i, timeButtons.OutsideTrigger);
    }
}
