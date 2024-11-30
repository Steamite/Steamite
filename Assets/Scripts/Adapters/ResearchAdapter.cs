using System;
using TMPro;
using UnityEngine;

public class ResearchAdapter : MonoBehaviour
{
    Action<float> researchProduction;
    Action<TMP_Text> researchDisplay;

    public void Init(Action<float> _researchProduction, Action<TMP_Text> _researchDisplay)
    {
        researchProduction += _researchProduction;
        researchDisplay += _researchDisplay;
    }

    public void DoProduction(float speed) => researchProduction?.Invoke(speed);
    public void DisplayResearch(TMP_Text text) => researchDisplay?.Invoke(text);
}

