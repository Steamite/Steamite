

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UIElements.PanelRenderer;

[RequireComponent(typeof(PanelRenderer))]
public class PanelRendererRoot : MonoBehaviour
{
    VisualElement root;
    public VisualElement Root => root;
    public PanelRenderer Renderer => GetComponent<PanelRenderer>();
    List<UIReloadCallback> callbacks = new();

    public void RegisterReload(UIReloadCallback callback) 
    {
        Renderer.RegisterUIReloadCallback(callback);
        callbacks.Add(callback);
    }
    private void OnEnable()
    {
        Renderer.RegisterUIReloadCallback(SaveRoot);
        foreach (var item in callbacks)
        {
            Renderer.RegisterUIReloadCallback(item);
        }
    }
    private void OnDisable()
    {
        Renderer.UnregisterUIReloadCallback(SaveRoot);
        foreach (var item in callbacks)
        {
            Renderer.UnregisterUIReloadCallback(item);
        }
    }

    void SaveRoot(PanelRenderer panelRenderer, VisualElement rootElement)
    {
        root = rootElement;
        OnUIReload();
    }

    protected virtual void OnUIReload()
    {

    }
}
