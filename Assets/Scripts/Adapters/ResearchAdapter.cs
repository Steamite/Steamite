using System;
using UnityEngine;

public struct ResearchDispayData
{
    public string name;
    public Color color;
    public string progress;
}
/// <summary>
/// Connects Baseassembly to <see cref="ResearchBackend"/>.
/// </summary>
public class ResearchAdapter : MonoBehaviour
{
    Action<float> researchProduction;

    /// <summary>
    /// Links research actions.
    /// </summary>
    /// <param name="_researchProduction"></param>
    /// <param name="_researchDisplay"></param>
    public void Init(Action<float> _researchProduction)
    {
        researchProduction += _researchProduction;
    }

    /// <summary>
    /// Triggered by research building,
    /// </summary>
    /// <param name="speed"></param>
    public void DoProduction(float speed) => researchProduction?.Invoke(speed);

}