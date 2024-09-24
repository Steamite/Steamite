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
        if (gameObject.activeSelf)
            GameObject.Find("Scene").GetComponent<Tick>().Unpause();
        else
            GameObject.Find("Scene").GetComponent<Tick>().ChangeGameSpeed(0);
        gameObject.SetActive(!gameObject.activeSelf);
        //menu.transform.parent.GetChild(1).gameObject.SetActive(menu.activeSelf);
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
        StartCoroutine(HideText());
    }
    IEnumerator HideText()
    {
        transform.parent.GetChild(transform.parent.childCount - 1).gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        transform.parent.GetChild(transform.parent.childCount - 1).gameObject.SetActive(false);
    }
}
