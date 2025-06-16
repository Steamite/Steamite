using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;

/// <summary>Holds references to the most important and frequented classes.</summary>
public class SceneRefs : MonoBehaviour
{
    #region Variables
    public static bool isInit => instance != null;
    static SceneRefs instance;
    [Header("GridObjects")]
    [SerializeField] GridTiles _gridTiles;
    [SerializeField] ClickableObjectFactory _objectFactory;
    [SerializeField] HumanUtil _humans;
    [SerializeField] JobQueue _jobQueue;
    [SerializeField] Tick _tick;

    [Header("Canvas")]
    [SerializeField] Transform _stats;
    [SerializeField] UIDocument _miscellaneous;
    [SerializeField] InfoWindow _infoWindow;

    [Header("Adapters")]
    [SerializeField] ResearchAdapter _researchAdapter;


    [SerializeReference] List<MonoBehaviour> afterLoads = new();
    [SerializeReference] List<MonoBehaviour> beforeLoads = new();
    #endregion

    #region Getters
    public static GridTiles gridTiles => instance._gridTiles;
    public static ClickableObjectFactory objectFactory => instance._objectFactory;
    public static HumanUtil humans => instance._humans;
    public static JobQueue jobQueue => instance._jobQueue;
    public static Tick tick => instance._tick;

    public static Transform BottomBar => instance._stats;
    public static UIDocument miscellaneous => instance._miscellaneous;
    public static InfoWindow infoWindow => instance._infoWindow;

    public static ResearchAdapter researchAdapter => instance._researchAdapter;
    #endregion

    [RuntimeInitializeOnLoadMethod]
    static void ReloadDomain() => instance = null;

    /// <summary>Registers the <see cref="instance"/></summary>
    public IEnumerator BeforeLoad()
    {
        instance = this;
        
        foreach (IBeforeLoad beforeLoad in instance.beforeLoads)
            yield return beforeLoad.Init();
        beforeLoads = null;
    }

    public static void FinishLoad()
    {
        foreach (IAfterLoad afterLoad in instance.afterLoads.Cast<IAfterLoad>())
            afterLoad.Init();
        instance.afterLoads = null;
        MyGrid.Init();
    }

    /// <summary>
    /// Displays/replaces the message.
    /// </summary>
    /// <param name="text">Message text.</param>
    public static void ShowMessage(string text) => instance.ShowMsg(text);

    /// <inheritdoc cref="ShowMessage(string)"/>
    void ShowMsg(string text)
    {
        StopAllCoroutines();
        StartCoroutine(MessageToggle(text));
    }

    /// <summary>
    /// Shows message for 2 seconds.
    /// </summary>
    /// <param name="text">Message text.</param>
    IEnumerator MessageToggle(string text)
    {
        ((Label)miscellaneous.rootVisualElement[1]).text = text;
        yield return new WaitForSecondsRealtime(2f);
        ((Label)miscellaneous.rootVisualElement[1]).text = "";
    }
}