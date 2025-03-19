using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>Holds references to the most important and frequented classes.</summary>
public class SceneRefs : MonoBehaviour
{
    #region Variables
    static SceneRefs instance;
    [Header("GridObjects")]
    [SerializeField] GridTiles _gridTiles;
    [SerializeField] ClickabeObjectFactory _objectFactory;
    [SerializeField] HumanUtil _humans;
    [SerializeField] JobQueue _jobQueue;
    [SerializeField] Tick _tick;

    [Header("Canvas")]
    [SerializeField] Transform _stats;
    [SerializeField] Transform _buildMenu;
    [SerializeField] Transform _miscellaneous;
    [SerializeField] InfoWindow _infoWindow;

    [Header("Adapters")]
    [SerializeField] ResearchAdapter _researchAdapter;

    bool messageShown = false;
    #endregion

    #region Getters
    public static GridTiles gridTiles => instance._gridTiles;
    public static ClickabeObjectFactory objectFactory => instance._objectFactory;
    public static HumanUtil humans => instance._humans;
    public static JobQueue jobQueue => instance._jobQueue;
    public static Tick tick => instance._tick;

    public static Transform stats => instance._stats;
    public static Transform buildMenu => instance._buildMenu;
    public static Transform miscellaneous => instance._miscellaneous;
    public static InfoWindow infoWindow => instance._infoWindow;

    public static ResearchAdapter researchAdapter => instance._researchAdapter;
    #endregion

    /// <summary>Registers the <see cref="instance"/></summary>
    public void Init()
    {
        instance = this;
    }

    /// <summary>
    /// Displays/replaces the message.
    /// </summary>
    /// <param name="text">Message text.</param>
    public static void ShowMessage(string text) => instance.ShowMsg(text);

    /// <inheritdoc cref="ShowMessage(string)"/>
    void ShowMsg(string text)
    {
        if (messageShown)
            StopCoroutine("MessageToggle");
        StartCoroutine(MessageToggle(text));
    }

    /// <summary>
    /// Shows message for 2 seconds.
    /// </summary>
    /// <param name="text">Message text.</param>
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