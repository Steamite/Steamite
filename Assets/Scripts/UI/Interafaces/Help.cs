using UnityEngine;
using UnityEngine.UIElements;


public class Help : PanelRendererRoot
{
    VisualElement controls;
    protected override void OnUIReload()
    {
        controls = Root.Q<VisualElement>("Controlls");
        
        
        Root.Q<Button>("help-button").clicked += OpenHelp;
        controls.Q<Button>("Save-Close-Button").clicked += CloseHelp;
    }

    void OpenHelp()
    {
        MainShortcuts.DisableInput(true);
        controls.style.display = DisplayStyle.Flex;
    }
    void CloseHelp()
    {
        MainShortcuts.EnableInput();
        controls.style.display = DisplayStyle.None;
    }
}
