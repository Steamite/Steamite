using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class BaseOverlay : MonoBehaviour
{
    public Gradient gradient;
    public InputAction input;
    public Sprite sprite;

    public abstract void Overlay(NativeArray<float> overlay);

    public void SetInputIndex(int i)
    {
        input.Disable();
        string processorString = $"scale(factor={i})";

        input.ApplyBindingOverride(0, new InputBinding
        {
            overrideProcessors = processorString
        });

        input.Enable();
    }
}