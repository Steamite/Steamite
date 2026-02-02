using StartMenu;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public abstract class DoubleWindow : MonoBehaviour, IToolkitController, IGridMenu
{
    protected VisualElement menu;
    protected bool isMainMenu;
    public void CloseWindow(ClickEvent _ = null)
    {
        if (isMainMenu)
            gameObject.GetComponent<MyMainMenu>().CloseWindow();
        else
            menu.style.display = DisplayStyle.None;
    }

    public virtual void Init(VisualElement root)
    {
        isMainMenu = false;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).buildIndex == 2)
                isMainMenu = true;
        }
    }

    public bool IsOpen()
    {
        if (isMainMenu)
        {
            return false;
        }
        return menu.style.display == DisplayStyle.Flex;
    }

    public abstract void OpenWindow(ClickEvent _ = null);

    public abstract void UpdateButtonState();
}
