using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class MyMainMenu : MonoBehaviour
{
    VisualElement document;

    void Start()
    {
        document = gameObject.GetComponent<UIDocument>().rootVisualElement;

        var uxmlField = document.Q<CustomRadioButtonGroup>("Saves");
        uxmlField.Init();


        Button saveClose = document.Q<Button>("Save-Close-Button");
        saveClose.RegisterCallback<ClickEvent>((_) => document.Q<VisualElement>("Load-Menu").style.display = DisplayStyle.None);
    }


}
