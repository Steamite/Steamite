using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UIElements;

public abstract class InitilizablePanelRenderer : PanelRendererRoot
{

    protected void AddOnLoad(ref Action a) => a += () => RegisterReload(OnDataLoad);
    protected void RegisterLoad() => RegisterReload(OnDataLoad);

    void OnDataLoad(PanelRenderer panelRenderer, VisualElement rootElement) => OnDataLoadLogic();

    protected abstract void OnDataLoadLogic();
}
