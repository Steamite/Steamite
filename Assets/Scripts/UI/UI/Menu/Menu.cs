using System;
using System.Collections.Generic;
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

    VisualElement menu;

    public void Init(Action<string> save, ref Action afterSave)
    {
        gameObject.SetActive(true);
        saveDialog.Init(save);
        ((IToolkitController)loadMenu).Init(uiDocument.rootVisualElement);
        ((IToolkitController)confrimWindow).Init(uiDocument.rootVisualElement);
        menu = uiDocument.rootVisualElement.Q<VisualElement>("Menu");
        menu.Q<Button>("Close").RegisterCallback<ClickEvent>(Toggle);
        menu.Q<Button>("Main-Menu").RegisterCallback<ClickEvent>(GoToMainMenu);
        menu.Q<Button>("Quit").RegisterCallback<ClickEvent>(DoQuit);

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
        else if (UIRefs.research.window.style.display == DisplayStyle.Flex)
            UIRefs.research.CloseWindow();
        else if (UIRefs.trading.window.style.display == DisplayStyle.Flex)
            UIRefs.trading.CloseWindow();
        else
            return true;

        return false;
    }

    public void Toggle(ClickEvent _ = null)
    {
        if(Constrains())
        {
            bool menuIsOn = menu.style.display == DisplayStyle.Flex;
            if (menuIsOn)
            {
                MainShortcuts.EnableInput();
                SceneRefs.tick.Unpause();
            }
            else
            {
                MainShortcuts.DisableInput(false);
                SceneRefs.tick.ChangeGameSpeed(0);
            }
            UIRefs.levelCamera.enabled = menuIsOn;
            UIRefs.levelCamera.mainCamera.GetComponent<PhysicsRaycaster>().enabled = menuIsOn;
            UIRefs.levelCamera.mainCamera.GetComponent<Physics2DRaycaster>().enabled = menuIsOn;
            menu.style.display = menuIsOn ? DisplayStyle.None : DisplayStyle.Flex;
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
