using UnityEngine;
using UnityEngine.UIElements;


public class Help : PanelRendererRoot
{
    protected override void OnUIReload()
    {
        VisualElement controls = Root.Q<VisualElement>("Controlls");
        
        
        Root.Q<Button>("help-button").clicked += () => { controls.style.display = DisplayStyle.Flex; };
        controls.Q<Button>("Save-Close-Button").clicked += () => { controls.style.display = DisplayStyle.None; };
    }
}
