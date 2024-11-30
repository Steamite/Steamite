using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    static CanvasManager instance;
    [Header("Canvases")]
    [SerializeField]Transform _stats;
    [SerializeField]Transform _buildMenu;
    [SerializeField]Transform _miscellaneous;
    [SerializeField]InfoWindow _infoWindow;
    public static Transform stats => instance._stats;
    public static Transform buildMenu => instance._buildMenu;
    public static Transform miscellaneous => instance._miscellaneous;
    public static InfoWindow infoWindow => instance._infoWindow;


    [Header("Adapters")]
    [SerializeField] ResearchAdapter _researchAdapter;
    public static ResearchAdapter researchAdapter => instance._researchAdapter;




    //public static Action<>
    bool messageShown = false;



#if UNITY_EDITOR_WIN
    [InitializeOnLoadMethod]
    static void In()
    {
        GameObject.Find("UI canvas")?.GetComponent<CanvasManager>().Init();
    }
#endif

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
