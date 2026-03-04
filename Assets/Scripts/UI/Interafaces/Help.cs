using UnityEngine;
using UnityEngine.UIElements;

public class Help : MonoBehaviour
{
    private void Awake()
    {
        UIDocument document = GetComponent<UIDocument>();
        VisualElement visualElement = document.rootVisualElement.Q<VisualElement>("Controlls");
        document.rootVisualElement.Q<Button>("help-button").clicked += () => { visualElement.style.display = DisplayStyle.Flex; };
        visualElement.Q<Button>("Save-Close-Button").clicked += () => { visualElement.style.display = DisplayStyle.None; };
    }
}
