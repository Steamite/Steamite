using UnityEngine;
using UnityEngine.UIElements;


public class Help : FullscreenWindow
{
    VisualElement controls;
    protected override void OnUIReload()
    {
        controls = Root.Q<VisualElement>("Controlls");
        
        
        Root.Q<Button>("help-button").clicked += OpenWindow;
        controls.Q<Button>("Save-Close-Button").clicked += CloseWindow;
    }

    public override void OpenWindow()
    {
        UIRefs.FullscreenConstraint();
        IsOpen = true;
        MainShortcuts.DisableInput();
        controls.style.display = DisplayStyle.Flex;
        SceneRefs.Tick.UIWindowToggle(false);
    }

    public override void CloseWindow()
    {
        IsOpen = false;
        MainShortcuts.EnableInput();
        controls.style.display = DisplayStyle.None;
        SceneRefs.Tick.UIWindowToggle(true);
    }

    protected override void OnDataLoadLogic()
    {
    }
}
