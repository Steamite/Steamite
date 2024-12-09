using System;
using TMPro;
using UnityEngine;

public struct ResearchDispayData
{
    public string name;
    public Color color;
    public string progress;
}

public class ResearchAdapter : MonoBehaviour
{
    Action<float> researchProduction;
    Func<ResearchDispayData> researchDisplay;

    public void Init(Action<float> _researchProduction, Func<ResearchDispayData> _researchDisplay)
    {
        researchProduction += _researchProduction;
        researchDisplay += _researchDisplay;
    }

    public void DoProduction(float speed) => researchProduction?.Invoke(speed);
    public ResearchDispayData DisplayResearch() => researchDisplay.Invoke();

}

