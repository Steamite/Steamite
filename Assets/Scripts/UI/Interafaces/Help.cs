using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRendererRoot))]
public class Help : MonoBehaviour
{
    private void Awake()
    {
        PanelRendererRoot document = GetComponent<PanelRendererRoot>();
        document.RegisterReload(InitButtons);
    }
    void InitButtons(VisualElement element)
    {
        VisualElement controls = element.Q<VisualElement>("Controlls");
        element.Q<Button>("help-button").clicked += () => { controls.style.display = DisplayStyle.Flex; };
        controls.Q<Button>("Save-Close-Button").clicked += () => { controls.style.display = DisplayStyle.None; };
    }
}
