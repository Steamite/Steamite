using UnityEngine;
using UnityEngine.UIElements;

public abstract class FullscreenWindow : MonoBehaviour
{
    public VisualElement window;
    public bool isOpen = false;
    public virtual void GetWindow()
    {
        window = gameObject.GetComponent<UIDocument>().rootVisualElement;
        window.style.display = DisplayStyle.None;
        window[0].style.display = DisplayStyle.Flex;
    }
    public void ToggleWindow()
    {
        if (isOpen)
        {
            CloseWindow();
        }
        else
        {
            OpenWindow();
        }
    }

    /// <summary>Opening the window, disables shortcuts and hides info window.</summary>
    public virtual void OpenWindow()
    {
        isOpen = true;
        SceneRefs.GridTiles.MarkPipeCheckpoint();
        SceneRefs.GridTiles.DeselectObjects();
        MainShortcuts.DisableInput();
        SceneRefs.InfoWindow?.Close();
        window.style.display = DisplayStyle.Flex;
        SceneRefs.Tick.UIWindowToggle(false);
    }

    /// <summary>Closing the window, enables shortcuts.</summary>
    public virtual void CloseWindow()
    {
        isOpen = false;
        MainShortcuts.EnableInput();
        window.style.display = DisplayStyle.None;
        SceneRefs.Tick.UIWindowToggle(true);
    }
}
