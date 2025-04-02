using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class FullscreenWindow : MonoBehaviour
{
    public VisualElement window;
    public virtual void GetWindow()
    {
        window = gameObject.GetComponent<UIDocument>().rootVisualElement;
        window.style.display = DisplayStyle.None;
        window[0].style.display = DisplayStyle.Flex;
        //CloseWindow();
    }
    public void ToggleWindow()
    {
        if (window.style.display == DisplayStyle.Flex)
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
        SceneRefs.gridTiles.DeselectObjects();
        MainShortcuts.DisableInput();
        SceneRefs.infoWindow.Close();
        window.style.display = DisplayStyle.Flex; 
        SceneRefs.tick.UIWindowToggle(false);
	}

	/// <summary>Closing the window, enables shortcuts.</summary>
	public virtual void CloseWindow()
    {
        MainShortcuts.EnableInput();
        window.style.display = DisplayStyle.None;
        SceneRefs.tick.UIWindowToggle(true);
    }
}
