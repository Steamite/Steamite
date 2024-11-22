using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void Toggle()
    {

        if (CanvasManager.research.window.activeSelf)
        {
            CanvasManager.research.CloseWindow();
            return;
        }
        else if (CanvasManager.trade.window.activeSelf)
        {
            CanvasManager.trade.CloseWindow();
            return;
        }

        if (gameObject.activeSelf)
        {
            MainShortcuts.EnableInput();
            GameObject.Find("Scene").GetComponent<Tick>().Unpause();
        }
        else
        {
            MainShortcuts.DisableInput(false);
            GameObject.Find("Scene").GetComponent<Tick>().ChangeGameSpeed(0);
        }
        SceneRefs.levelCamera.enabled = gameObject.activeSelf;
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
