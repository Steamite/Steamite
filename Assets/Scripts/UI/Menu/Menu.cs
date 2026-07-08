using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class Menu : PanelRendererRoot
{
    [SerializeField] ConfirmWindow confrimWindow;

    [SerializeField] MonoBehaviour settings;

    public VisualElement MenuContainer { get; set; }
    bool IsOpen { get; set; }

    protected override void OnUIReload()
    {
        MenuContainer = Root.Q<VisualElement>("Container");
        if(!IsOpen)
            MenuContainer.style.display = DisplayStyle.None;

    }

    public void Init(Action<string> save, ref Action afterSave)
    {
        gameObject.SetActive(true);
        UIRefs.SaveDialog.Init(save);
        ((IToolkitController)UIRefs.LoadMenu).Init(Root);
        ((IToolkitController)settings).Init(Root);
        confrimWindow.Init(Root);

        /*menuContainer = PanelRenderer.rootVisualElement.Q<VisualElement>("Container");
        menuContainer.style.display = DisplayStyle.None;*/
        MenuContainer.Q<Button>("Close").RegisterCallback<ClickEvent>(Toggle);
        MenuContainer.Q<Button>("Main-Menu").RegisterCallback<ClickEvent>(GoToMainMenu);
        MenuContainer.Q<Button>("Quit").RegisterCallback<ClickEvent>(DoQuit);

        afterSave += ((IGridMenu)UIRefs.LoadMenu).UpdateButtonState;
        afterSave += () => ((IGridMenu)UIRefs.SaveDialog).CloseWindow();
    }

    public void Toggle(ClickEvent _ = null)
    {
        if (UIRefs.WindowConstraint())
        {
            bool menuIsOn = MenuContainer.style.display == DisplayStyle.Flex;
            if (menuIsOn)
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
            UIRefs.LevelCamera.enabled = menuIsOn;
            UIRefs.LevelCamera.mainCamera.GetComponent<PhysicsRaycaster>().enabled = menuIsOn;
            UIRefs.LevelCamera.mainCamera.GetComponent<Physics2DRaycaster>().enabled = menuIsOn;
            MenuContainer.style.display = menuIsOn ? DisplayStyle.None : DisplayStyle.Flex;
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
