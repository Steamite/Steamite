using System;
using Unity.Profiling;
using UnityEngine;
public class FPSCounter : MonoBehaviour
{
    private ProfilerRecorder activeWorkRecorder;
    private ProfilerRecorder totalTimeRecorder;
    int i;

    [SerializeField]int targetFPS = 60;

    public Action<double, double> onUpdate;

    void OnEnable()
    {

        Application.targetFrameRate = targetFPS;
        QualitySettings.vSyncCount = 0;
        // Tracks pure CPU execution time, ignoring VSync and frame limits
        activeWorkRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "CPU Main Thread Frame Time");

        // Tracks the entire frame duration, including artificial sleep
        totalTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "CPU Total Frame Time");
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
            onUpdate?.Invoke(realWorkMs, totalMs);
        }
    }

    

    void OnDisable()
    {
        activeWorkRecorder.Dispose();
        totalTimeRecorder.Dispose();
    }
}
