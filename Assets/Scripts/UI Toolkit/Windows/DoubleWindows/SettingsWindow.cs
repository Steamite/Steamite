using StartMenu;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Settings
{

    public class SettingsWindow : DoubleWindow
    {
        Button saveSettings;

        public override void Init(VisualElement root)
        {
            menu = root.Q<VisualElement>("Settings");
            menu.Q<Button>("Close-Button").RegisterCallback<ClickEvent>(CloseWindow);
            //menu.Q<>
        }

        public override void OpenWindow(ClickEvent _ = null)
        {
            
        }

        public override void UpdateButtonState()
        {
            throw new NotImplementedException();
        }
    }
}
