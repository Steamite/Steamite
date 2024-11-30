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

        //document.Q<SaveRadioGroup>("Saves").Init(null);
        gameObject.GetComponent<NewGameWindow>().Init(document);
        gameObject.GetComponent<MapGeneration>().Init(document);

        // opening load menu
        Button button = document.Q<Button>("Load-Game-Button");
        button.RegisterCallback<ClickEvent>((_) => document.Q<VisualElement>("Load-Menu").style.display = DisplayStyle.Flex);

        // closing load menu
        button = document.Q<Button>("Save-Close-Button");
        button.RegisterCallback<ClickEvent>((_) => document.Q<VisualElement>("Load-Menu").style.display = DisplayStyle.None);
        
        // exiting game
        button = document.Q<Button>("Exit-Button");
        button.RegisterCallback<ClickEvent>((_) => Application.Quit());
    }
}
