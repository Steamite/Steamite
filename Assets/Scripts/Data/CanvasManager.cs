using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    [Header("Canvases")]
    [SerializeField] public Transform stats;
    [SerializeField] public Transform buildMenu;
    [SerializeField] public Transform miscellaneous;
    [SerializeField] public UIOverlay overlays;
    [SerializeField] public ResearchUI research;
    [SerializeField] public Menu pauseMenu;
    [SerializeField] public InfoWindow infoWindow;
    [SerializeField] public Trade tradeWindow;

    bool messageShown = false;
    void InitCanvases()
    {
        gameObject.SetActive(true);
    }

    public void ShowMessage(string text)
    {
        if (messageShown)
            StopCoroutine("MessageToggle");
        StartCoroutine(MessageToggle(text));
    }

    IEnumerator MessageToggle(string text)
    {
        messageShown = true;
        GameObject g = MyGrid.canvasManager.miscellaneous.GetChild(1).gameObject;
        g.GetComponent<TMP_Text>().text = text;
        g.SetActive(true);
        yield return new WaitForSecondsRealtime(2f);
        g.SetActive(false);
        messageShown = false;
    }
}
