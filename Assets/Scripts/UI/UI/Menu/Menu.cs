using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class Menu : MonoBehaviour
{
    [SerializeField] SaveDialog saveDialog;
    [SerializeField] ConfirmWindow confrimWindow;
    [SerializeReference] MonoBehaviour loadMenu;

    [SerializeField] UIDocument uiDocument;

    VisualElement menuContainer;

    public void Init(Action<string> save, ref Action afterSave)
    {
        gameObject.SetActive(true);
        saveDialog.Init(save);
        ((IToolkitController)loadMenu).Init(uiDocument.rootVisualElement);
        confrimWindow.Init(uiDocument.rootVisualElement);
        menuContainer = uiDocument.rootVisualElement.Q<VisualElement>("Container");
        menuContainer.Q<Button>("Close").RegisterCallback<ClickEvent>(Toggle);
        menuContainer.Q<Button>("Main-Menu").RegisterCallback<ClickEvent>(GoToMainMenu);
        menuContainer.Q<Button>("Quit").RegisterCallback<ClickEvent>(DoQuit);

        afterSave += ((IGridMenu)loadMenu).UpdateButtonState;
        afterSave += () => ((IGridMenu)saveDialog).CloseWindow();
    }

    public bool Constrains()
    {
        if (ConfirmWindow.window.opened)
            ConfirmWindow.window.Close(false);
        else if (saveDialog.opened)
            saveDialog.CloseWindow();
        else if (((IGridMenu)loadMenu).IsOpen())
            ((IGridMenu)loadMenu).CloseWindow();
        else if (UIRefs.ResearchWindow.isOpen)
            UIRefs.ResearchWindow.CloseWindow();
        else if (UIRefs.TradingWindow.isOpen)
            UIRefs.TradingWindow.CloseWindow();
        else if (SceneRefs.GridTiles.activeControl != ControlMode.nothing)
            SceneRefs.GridTiles.BreakAction();
        else
            return true;

        return false;
    }

    public void Toggle(ClickEvent _ = null)
    {
        if (Constrains())
        {
            bool menuIsOn = menuContainer.style.display == DisplayStyle.Flex;
            if (menuIsOn)
            {
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
            menuContainer.style.display = menuIsOn ? DisplayStyle.None : DisplayStyle.Flex;
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
