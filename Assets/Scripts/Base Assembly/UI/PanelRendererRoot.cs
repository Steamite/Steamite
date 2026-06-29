using System;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public class PanelRendererRoot : MonoBehaviour
{
    VisualElement root;
    public VisualElement Root => root;
    public PanelRenderer Renderer => GetComponent<PanelRenderer>();
    Action<VisualElement> reload;

    public void RegisterReload(Action<VisualElement> a) => reload += a;
    private void OnEnable()
    {
        Renderer.RegisterUIReloadCallback(SaveRoot);
    }
    private void OnDisable()
    {
        Renderer.UnregisterUIReloadCallback(SaveRoot);
    }

    private void SaveRoot(PanelRenderer panelRenderer, VisualElement rootElement)
    {
        root = rootElement;
        reload?.Invoke(root);
    }
}
