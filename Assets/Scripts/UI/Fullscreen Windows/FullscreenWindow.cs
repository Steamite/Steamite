using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullscreenWindow : MonoBehaviour
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
        // open view Window
        MyGrid.gridTiles.DeselectObjects();
        EventHandler.DisableInput();
        MyGrid.canvasManager.infoWindow.gameObject.SetActive(false);
        window.SetActive(true);

    }

    //Closes the research UI
    public virtual void CloseWindow()
    {
        window.SetActive(false);
        EventHandler.EnableInput();
        MyGrid.canvasManager.infoWindow.gameObject.SetActive(false);
    }
}
