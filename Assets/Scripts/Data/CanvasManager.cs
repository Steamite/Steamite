using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    static CanvasManager instance;
    [Header("Canvases")]
    [SerializeField] public Transform _stats;
    [SerializeField] public Transform _buildMenu;
    [SerializeField] public Transform _miscellaneous;
    [SerializeField] public ResearchUI _research;
    [SerializeField] public Menu _pauseMenu;
    [SerializeField] public InfoWindow _infoWindow;
    [SerializeField] public Trade _trade;

    public static Transform stats => instance._stats;
    public static Transform buildMenu => instance._buildMenu;
    public static Transform miscellaneous => instance._miscellaneous;
    public static ResearchUI research => instance._research;
    public static Menu pauseMenu => instance._pauseMenu;
    public static InfoWindow infoWindow => instance._infoWindow;
    public static Trade trade => instance._trade;


    bool messageShown = false;


    public void Init()
    {
        instance = this;
    }

    public static void ToggleCanvases()
    {
        instance.gameObject.SetActive(!instance.gameObject.activeSelf);
    }

    public static void ShowMessage(string text)
    {
        instance.ShowMsg(text);
    }
    void ShowMsg(string text)
    {
        if (messageShown)
            StopCoroutine("MessageToggle");
        StartCoroutine(MessageToggle(text));
    }

    IEnumerator MessageToggle(string text)
    {
        messageShown = true;
        GameObject g = miscellaneous.GetChild(0).gameObject;
        g.GetComponent<TMP_Text>().text = text;
        g.SetActive(true);
        yield return new WaitForSecondsRealtime(2f);
        g.SetActive(false);
        messageShown = false;
    }
}
