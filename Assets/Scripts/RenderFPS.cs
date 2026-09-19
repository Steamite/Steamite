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

    private void UpdateText(double realWorkMs, double totalMs)
    {
        if (!render)
            return;
        double waitMs = totalMs - realWorkMs;

        text.SetText("Active Work: {0:2} ms \nWait: {1:2} ms\nTotal: {2:2} ms", (float)realWorkMs, (float)waitMs, (float)totalMs);
    }
}
