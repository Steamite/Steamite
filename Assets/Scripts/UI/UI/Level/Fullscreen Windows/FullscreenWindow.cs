using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FullscreenWindow : MonoBehaviour
{
    public GameObject window;
    public void ToggleWindow()
    {
        if (window.gameObject.activeSelf)
        {
            CloseWindow();
        }
        else
        {
            OpenWindow();
        }
    }

    //Opens the research UI
    public virtual void OpenWindow()
    {
        if (!window.activeSelf)
        {
            SceneRefs.gridTiles.DeselectObjects();
            MainShortcuts.DisableInput();
            SceneRefs.infoWindow.gameObject.SetActive(false);
            window.SetActive(true);
        }
        
    }

    //Closes the research UI
    public virtual void CloseWindow()
    {
        if (window.activeSelf)
        {
            MainShortcuts.EnableInput();
            SceneRefs.infoWindow.gameObject.SetActive(false);
            window.SetActive(false);
        }
    }
}
