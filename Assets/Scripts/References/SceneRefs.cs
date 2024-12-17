using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SceneRefs : MonoBehaviour
{
    static SceneRefs instance;
    [Header("GridObjects")]
    [SerializeField] GridTiles _gridTiles;
    [SerializeField] ClickabeObjectFactory _objectFactory;
    [SerializeField] Humans _humans;
    [SerializeField] Tick _tick;

    public static GridTiles gridTiles => instance._gridTiles;
    public static ClickabeObjectFactory objectFactory => instance._objectFactory;
    public static Humans humans => instance._humans;
    public static Tick tick => instance._tick;


    [Header("Canvas")]
    [SerializeField] Transform _stats;
    [SerializeField] Transform _buildMenu;
    [SerializeField] Transform _miscellaneous;
    [SerializeField] InfoWindow _infoWindow;

    public static Transform stats => instance._stats;
    public static Transform buildMenu => instance._buildMenu;
    public static Transform miscellaneous => instance._miscellaneous;
    public static InfoWindow infoWindow => instance._infoWindow;

    [Header("Adapters")]
    [SerializeField] ResearchAdapter _researchAdapter;
    public static ResearchAdapter researchAdapter => instance._researchAdapter;

    bool messageShown = false;


    public void Init()
    {
        instance = this;
    }

    public void Clear()
    {
        Destroy(this);
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