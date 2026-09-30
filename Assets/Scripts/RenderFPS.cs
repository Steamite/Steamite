using System;
using TMPro;
using UnityEngine;

public class RenderFPS : MonoBehaviour
{

    [SerializeField] TMP_Text text;

    [SerializeField] bool render;

    private void Awake()
    {
        Camera.main.gameObject.SetActive(render);
        GetComponent<FPSCounter>().onUpdate += UpdateText;
    }

    private void UpdateText(double realWorkMs, double totalMs, double gpuMS)
    {
        if (!render)
            return;
        double waitMs = totalMs - realWorkMs;

        text.SetText("Active Work: {0:2} ms \nGPU: {1:2} ms\nWait: {2:2} ms\nTotal: {3:2} ms", (float)realWorkMs, (float)gpuMS, (float)waitMs, (float)totalMs);
    }
}
