using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
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
