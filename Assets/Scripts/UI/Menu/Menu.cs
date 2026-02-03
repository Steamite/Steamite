using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class Menu : MonoBehaviour
{
    [SerializeField] ConfirmWindow confrimWindow;

    [SerializeField] UIDocument uiDocument;

    public VisualElement menuContainer;

    [SerializeField] MonoBehaviour settings;

    public void Init(Action<string> save, ref Action afterSave)
    {
        gameObject.SetActive(true);
        UIRefs.SaveDialog.Init(save);
        ((IToolkitController)UIRefs.LoadMenu).Init(uiDocument.rootVisualElement);
        ((IToolkitController)settings).Init(uiDocument.rootVisualElement);
        confrimWindow.Init(uiDocument.rootVisualElement);
        menuContainer = uiDocument.rootVisualElement.Q<VisualElement>("Container");
        menuContainer.Q<Button>("Close").RegisterCallback<ClickEvent>(Toggle);
        menuContainer.Q<Button>("Main-Menu").RegisterCallback<ClickEvent>(GoToMainMenu);
        menuContainer.Q<Button>("Quit").RegisterCallback<ClickEvent>(DoQuit);

        afterSave += ((IGridMenu)UIRefs.LoadMenu).UpdateButtonState;
        afterSave += () => ((IGridMenu)UIRefs.SaveDialog).CloseWindow();
    }

    public void Toggle(ClickEvent _ = null)
    {
        if (UIRefs.WindowConstraint())
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
