using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void Toggle()
    {

        if (MyGrid.canvasManager.research.window.activeSelf)
        {
            MyGrid.canvasManager.research.CloseWindow();
            return;
        }
        else if (MyGrid.canvasManager.tradeWindow.window.activeSelf)
        {
            MyGrid.canvasManager.tradeWindow.CloseWindow();
            return;
        }

        if (gameObject.activeSelf)
            GameObject.Find("Scene").GetComponent<Tick>().Unpause();
        else
            GameObject.Find("Scene").GetComponent<Tick>().ChangeGameSpeed(0);
        MyGrid.sceneReferences.levelCamera.enabled = gameObject.activeSelf;
        gameObject.SetActive(!gameObject.activeSelf);
        Camera.main.GetComponent<PhysicsRaycaster>().enabled = !gameObject.activeSelf;
        Camera.main.GetComponent<Physics2DRaycaster>().enabled = !gameObject.activeSelf;
    }
    public void GoToMainMenu()
    {
        SceneManager.LoadSceneAsync(0);
    }
    public void DoQuit()
    {
        Application.Quit();
    }
    public void Save()
    {
        try
        {
            GameObject.Find("Scene").GetComponent<SaveController>().SaveGame();
        }
        catch
        {
            return;
        }
    }
}
