using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class Menu : InitilizablePanelRenderer
{
    [SerializeField] ConfirmWindow confirmWindow;

    [SerializeField] MonoBehaviour settings;


    public VisualElement MenuContainer { get; set; }
    Button close;
    Button mainMenu;
    Button quit;

    bool IsOpen { get; set; } = false;

    Action<string> save;

    protected override void OnUIReload()
    {
        MenuContainer = Root.Q<VisualElement>("Container");
        if(!IsOpen)
            MenuContainer.style.display = DisplayStyle.None;

        close = MenuContainer.Q<Button>("Close");
        mainMenu = MenuContainer.Q<Button>("Main-Menu");
        quit = MenuContainer.Q<Button>("Quit");
    }

    public void AddSaveAction(Action<string> save)
    {
        this.save = save;
        RegisterLoad();
    }

    protected override void OnDataLoadLogic()
    {
        UIRefs.SaveDialog.Init(save);
        ((IToolkitController)UIRefs.LoadMenu).Init(Root);
        ((IToolkitController)settings).Init(Root);
        confirmWindow.Init(Root);

        close.RegisterCallback<ClickEvent>(Toggle);
        mainMenu.RegisterCallback<ClickEvent>(GoToMainMenu);
        quit.RegisterCallback<ClickEvent>(DoQuit);
    }

    public void Toggle(ClickEvent _ = null)
    {
        if (UIRefs.WindowConstraint())
        {
            bool _isOpen = IsOpen;
            if (_isOpen)
            {
                if(((IGridMenu)settings).IsOpen())
                {
                    ((IGridMenu)settings).CloseWindow();
                    return;
                }
                MainShortcuts.EnableInput();
                SceneRefs.Tick.UIWindowToggle(true);
            }
            else
            {
                MainShortcuts.DisableInput(false);
                SceneRefs.Tick.UIWindowToggle(false);
            }
            UIRefs.LevelCamera.enabled = _isOpen;
            UIRefs.LevelCamera.mainCamera.GetComponent<PhysicsRaycaster>().enabled = _isOpen;
            UIRefs.LevelCamera.mainCamera.GetComponent<Physics2DRaycaster>().enabled = _isOpen;
            MenuContainer.style.display = _isOpen ? DisplayStyle.None : DisplayStyle.Flex;
            
            IsOpen = !_isOpen;
        }
    }

    public void GoToMainMenu(ClickEvent _)
    {
        SceneManager.LoadSceneAsync(0);
    }
    public void DoQuit(ClickEvent _)
    {
        Application.Quit();
    }
}
