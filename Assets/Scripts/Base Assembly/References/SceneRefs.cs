using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
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
    [SerializeField] CameraSceneMovement _cameraSceneMover;

    [Header("Adapters")]
    [SerializeField] ResearchAdapter _researchAdapter;
    /// <summary>
    /// Should be of type IQuestController
    /// </summary>
    [SerializeField] MonoBehaviour _questController;


    [SerializeReference] List<MonoBehaviour> afterLoads = new();
    [SerializeReference] List<MonoBehaviour> beforeLoads = new();
    #endregion

    #region Getters
    public static GridTiles GridTiles => instance._gridTiles;
    public static ClickableObjectFactory ObjectFactory => instance._objectFactory;
    public static HumanUtil Humans => instance._humans;
    public static JobQueue JobQueue => instance._jobQueue;
    public static Tick Tick => instance._tick;

    public static Transform Stats => instance._stats;
    public static UIDocument Miscellaneous => instance._miscellaneous;
    public static InfoWindow InfoWindow => instance._infoWindow;
    public static CameraSceneMovement CameraSceneMover => instance._cameraSceneMover;

    public static ResearchAdapter ResearchAdapter => instance._researchAdapter;
    public static IQuestController QuestController => instance._questController as IQuestController;
    #endregion

    [RuntimeInitializeOnLoadMethod]
    static void ReloadDomain() => instance = null;

    /// <summary>Registers the <see cref="instance"/></summary>
    public async Task BeforeLoad()
    {
        instance = this;

        foreach (IBeforeLoad beforeLoad in instance.beforeLoads)
            await beforeLoad.Init();
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
        ((Label)Miscellaneous.rootVisualElement[1]).text = text;
        yield return new WaitForSecondsRealtime(2f);
        ((Label)Miscellaneous.rootVisualElement[1]).text = "";
    }
}