

using System;
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
    [SerializeField] UIOverlay _overlays;
    [SerializeField] ClickableObjectFactory _objectFactory;
    [SerializeField] HumanUtil _humans;
    [SerializeField] JobQueue _jobQueue;
    [SerializeField] Tick _tick;

    [Header("Canvas")]
    [SerializeField] Transform _stats;
    //[SerializeField] NotificationController _miscellaneous;
    
    [SerializeField] InfoWindow _infoWindow;
    [SerializeField] CameraSceneMovement _cameraSceneMover;

    [Header("Adapters")]
    [SerializeField] ResearchAdapter _researchAdapter;
    /// <summary>
    /// Should be of type IQuestController
    /// </summary>
    [SerializeField] MonoBehaviour _questController;

    [SerializeReference] List<MonoBehaviour> beforeLoads = new();
    [SerializeReference] List<MonoBehaviour> afterLoads = new();
    #endregion

    #region Getters
    public static GridTiles GridTiles => instance._gridTiles;
    public static UIOverlay Overlays => instance._overlays;
    public static ClickableObjectFactory ObjectFactory => instance._objectFactory;
    public static HumanUtil Humans => instance._humans;
    public static JobQueue JobQueue => instance._jobQueue;
    public static Tick Tick => instance._tick;

    public static Transform Stats => instance._stats;
    //public static InfoWindow InfoWindow => instance._infoWindow;
    public static CameraSceneMovement CameraSceneMover => instance._cameraSceneMover;

    public static ResearchAdapter ResearchAdapter => instance._researchAdapter;
    public static IQuestController QuestController => instance._questController as IQuestController;
    #endregion

    [RuntimeInitializeOnLoadMethod]
    static void ReloadDomain() => instance = null;

    /// <summary>Registers the <see cref="instance"/></summary>
    public async Task BeforeLoad()
    {
        MyGrid.ReloadDomain();
        instance = this;

        foreach (IBeforeLoad beforeLoad in instance.beforeLoads)
            await beforeLoad.BeforeInit();
        beforeLoads = null;
    }

    public static void FinishLoad()
    {
        foreach (IAfterLoad afterLoad in instance.afterLoads.Cast<IAfterLoad>())
            afterLoad.AfterInit();
        instance.afterLoads = null;
        MyGrid.Init();
    }

    private void OnValidate()
    {
        for (int i = beforeLoads.Count - 1; i >= 0; i--)
        {
            if (beforeLoads[i] is not IBeforeLoad)
                beforeLoads.RemoveAt(i);
        }

        for (int i = afterLoads.Count - 1; i >= 0; i--)
        {
            if (afterLoads[i] is not IAfterLoad)
                afterLoads.RemoveAt(i);
        }

    }
}