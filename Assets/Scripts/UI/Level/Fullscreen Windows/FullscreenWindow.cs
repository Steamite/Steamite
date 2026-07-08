using System;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class FullscreenWindow : PanelRendererRoot
{
    public bool IsOpen { get; set; } = false;


    protected override void OnUIReload()
    {
        base.OnUIReload();
        if (IsOpen == false)
            Root.style.display = DisplayStyle.None;
    }

    protected void AddOnLoad(ref Action a) => a += () => RegisterReload(OnDataLoad);

    void OnDataLoad(PanelRenderer panelRenderer, VisualElement rootElement) => OnDataLoadLogic();
    protected abstract void OnDataLoadLogic();



    public void ToggleWindow()
    {
        if (IsOpen)
        {
            CloseWindow();
        }
        else
        {
            OpenWindow();
        }
    }

    /// <summary>Opening the window, disables shortcuts and hides info window.</summary>
    public virtual void OpenWindow()
    {
        if (UIRefs.FullscreenConstraint())
        {
            IsOpen = true;
            SceneRefs.GridTiles.DeselectObjects();
            MainShortcuts.DisableInput();
            InfoWindow.Window.Close();
            Root.style.display = DisplayStyle.Flex;
            SceneRefs.Tick.UIWindowToggle(false);
        }
    }

    /// <summary>Closing the window, enables shortcuts.</summary>
    public virtual void CloseWindow()
    {
        IsOpen = false;
        MainShortcuts.EnableInput();
        Root.style.display = DisplayStyle.None;
        SceneRefs.Tick.UIWindowToggle(true);
    }
}
