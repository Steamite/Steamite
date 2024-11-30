using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void Toggle()
    {
        if (UIRefs.research.window.activeSelf)
            UIRefs.research.CloseWindow();
        else if (UIRefs.trade.window.activeSelf)
            UIRefs.trade.CloseWindow();

        else
        {
            if (gameObject.activeSelf)
            {
                MainShortcuts.EnableInput();
                SceneRefs.tick.Unpause();
            }
            else
            {
                MainShortcuts.DisableInput(false);
                SceneRefs.tick.ChangeGameSpeed(0);
            }
            UIRefs.levelCamera.enabled = gameObject.activeSelf;
            UIRefs.levelCamera.mainCamera.GetComponent<PhysicsRaycaster>().enabled = gameObject.activeSelf;
            UIRefs.levelCamera.mainCamera.GetComponent<Physics2DRaycaster>().enabled = gameObject.activeSelf;
            gameObject.SetActive(!gameObject.activeSelf);
        }
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
