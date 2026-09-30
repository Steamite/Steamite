using System;
using System.IO;
using System.Linq;
using TMPro;
using Unity.Profiling;
using UnityEngine;
public class FPSCounter : MonoBehaviour
{
    private ProfilerRecorder activeWorkRecorder;
    private ProfilerRecorder totalTimeRecorder;
    private ProfilerRecorder gpuTimeRecorder;
    int i;

    [SerializeField] int FrameRate = -1;

    [SerializeField] bool canCount = false;
    [SerializeField] bool lockFPS = false;

    public bool CanCount
    {
        get => canCount;
        set
        {
            canCount = value;
            Toggle();
        }
    }
    public Action<double, double, double> onUpdate;


    private void Awake()
    {
        if(lockFPS)
        {
            QualitySettings.vSyncCount = -1;
            Application.targetFrameRate = FrameRate;
        }

        Toggle();
    }

    void Toggle()
    {
        enabled = canCount;
        GetComponent<RenderFPS>().enabled = canCount;
        GetComponent<PerformanceSaver>().enabled = canCount;
        transform.GetChild(0).gameObject.SetActive(canCount);
    }


    void OnEnable()
    {

        // Tracks pure CPU execution time, ignoring VSync and frame limits
        activeWorkRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "CPU Main Thread Frame Time");

        // Tracks the entire frame duration, including artificial sleep
        totalTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "CPU Total Frame Time");
        gpuTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "GPU Frame Time");
    }
    void Update()
    {

        if (activeWorkRecorder.Valid && totalTimeRecorder.Valid)
        {
            i++;
            if (i < 5)
                return;
            i = 0;



            double realWorkMs = activeWorkRecorder.LastValue / 1_000_000.0;
            double totalMs = totalTimeRecorder.LastValue / 1_000_000.0;
            double gpuMS = gpuTimeRecorder.LastValue / 1_000_000.0;
            onUpdate?.Invoke(realWorkMs, totalMs, gpuMS);
        }
    }

    

    void OnDisable()
    {
        activeWorkRecorder.Dispose();
        totalTimeRecorder.Dispose();
    }
}
