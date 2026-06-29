using StartMenu;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Settings
{

    public class SettingsWindow : DoubleWindow
    {
        List<Vector2Int> windowSizes = new()
        {
            new Vector2Int(1366, 768),
            new Vector2Int(1920, 1080),
            new Vector2Int(2560, 1440),
        };

        List<string> screenSettings = new()
        {
            "Fullscreen",
            "Borderless",
            "Windowed",
        };

        List<int> fps = new()
        {
            30,
            60,
            90,
            120,
            144,
        };

        Button resetSettings;
        Button saveSettings;
        Button revertSettings;
        
        SettingsData data;
        Toggle musicToggle;
        SliderInt masterVolumeSlider;
        SliderInt musicVolumeSlider;
        SliderInt effectVolumeSlider;

        Toggle vsyncToggle;
        DropdownField screenDropdown;
        DropdownField fpsDropdown;
        DropdownField resolutionDropdown;
    
        public override void Init(VisualElement root)
        {
            base.Init(root);
            menu = root.Q<VisualElement>("Settings");
            menu.Q<Button>("Close-Button").RegisterCallback<ClickEvent>(CloseWindow);
            
            musicToggle = menu.Q<Toggle>("Music-Toggle");
            musicToggle.RegisterValueChangedCallback((q) => 
            { 
                data.Mute = q.newValue; 
                UpdateButtonState(); 
            });

            masterVolumeSlider = menu.Q<SliderInt>("Master-Volume-Slider");
            masterVolumeSlider.RegisterValueChangedCallback((q) => 
            { 
                data.MasterVolume = q.newValue; 
                UpdateButtonState(); 
            });

            musicVolumeSlider = menu.Q<SliderInt>("Music-Volume-Slider");
            musicVolumeSlider.RegisterValueChangedCallback((q) => 
            { 
                data.MusicVolume = q.newValue; 
                UpdateButtonState(); 
            });

            effectVolumeSlider = menu.Q<SliderInt>("Effect-Volume-Slider");
            effectVolumeSlider.RegisterValueChangedCallback((q) => 
            { 
                data.EffectVolume = q.newValue; 
                UpdateButtonState(); 
            });

            vsyncToggle = menu.Q<Toggle>("VSync-Toggle");
            vsyncToggle.RegisterValueChangedCallback((q) => 
            { 
                data.VSync = q.newValue; 
                UpdateButtonState(); 
            });



            screenDropdown = menu.Q<DropdownField>("Fullscreen-Dropdown");
            screenDropdown.choices = screenSettings;
            screenDropdown.RegisterValueChangedCallback((q) =>
            {
                int i = screenSettings.IndexOf(q.newValue);
                if (i == 2)
                    i = 3;
                data.fullScreenMode = (FullScreenMode)i;
                UpdateButtonState();
            });


            fpsDropdown = menu.Q<DropdownField>("FPS-Dropdown");
            fpsDropdown.choices = fps.ConvertAll(f => f.ToString());
            fpsDropdown.RegisterValueChangedCallback((q) =>
            {
                data.MaxFPS = int.Parse(q.newValue);
                UpdateButtonState();
            });


            resolutionDropdown = menu.Q<DropdownField>("Resolution-Dropdown");
            resolutionDropdown.choices = windowSizes.ConvertAll(res => $"{res.x}x{res.y}");
            resolutionDropdown.RegisterValueChangedCallback((q) => 
            {
                string[] list = q.newValue.Split('x');
                data.Width = int.Parse(list[0]);
                data.Height = int.Parse(list[1]);
                UpdateButtonState();
            });

            resetSettings = menu.Q<Button>("Reset-Settings-Button");
            resetSettings.clicked += ResetSettings;

            saveSettings = menu.Q<Button>("Save-Settings-Button");
            saveSettings.clicked += SaveSettings;

            revertSettings = menu.Q<Button>("Revert-Settings-Button");
            revertSettings.clicked += RevertSettings;// = menu.Q<Button>("Revert-Settings-Button");

            Button settingsButton = root.Q<Button>("Settings-Button");
            settingsButton.RegisterCallback<ClickEvent>(OpenWindow);

            if (!isMainMenu)
            {
                menu[0].AddToClassList("game-window");
            }
        }

        public override void OpenWindow(ClickEvent _ = null)
        {
            LoadFromSettings(Settings.GetData());

            if (isMainMenu)
            {
                gameObject.GetComponent<MyMainMenu>().OpenWindow("settings");
            }
            else
            {
                menu.style.display = DisplayStyle.Flex;
            }
        }
        public override void CloseWindow(ClickEvent _ = null)
        {
            if (!data.Equals(Settings.GetData()))
            {
                ConfirmWindow.window.Open(
                    () =>
                    {
                        LoadFromSettings(Settings.GetData());
                        base.CloseWindow(_);
                    },
                    "Unsaved Changes",
                    $"You have unsaved changes, do you want to discard them?",
                    "discard",
                    "cancel");
            }
            else
            {
                base.CloseWindow(_);
            }
        }

        void LoadFromSettings(SettingsData _data)
        {
            data = _data;
            masterVolumeSlider.SetValueWithoutNotify(data.MasterVolume);
            musicVolumeSlider.SetValueWithoutNotify(data.MusicVolume);
            effectVolumeSlider.SetValueWithoutNotify(data.EffectVolume);
            musicToggle.SetValueWithoutNotify(data.Mute);
            vsyncToggle.SetValueWithoutNotify(data.VSync);

            int i = (int)data.fullScreenMode;
            if(i == 3)
                i = 2;
            screenDropdown.SetValueWithoutNotify(screenSettings[i]);
            fpsDropdown.SetValueWithoutNotify(data.MaxFPS.ToString());
            resolutionDropdown.SetValueWithoutNotify($"{data.Width}x{data.Height}");
            UpdateButtonState();
        }

        public override void UpdateButtonState()
        {
            bool b = !data.Equals(Settings.GetData());

            saveSettings.ToggleStyleButton(b);
            saveSettings.enabledSelf = b;

            revertSettings.ToggleStyleButton(b);
            revertSettings.enabledSelf = b;
        }

        void SaveSettings()
        {
            Settings.TestSettings(data);
            UpdateButtonState();
        }

        void RevertSettings()
        {
            ConfirmWindow.window.Open(
                () => { LoadFromSettings(Settings.GetData()); },
                "Revert settings?",
                "Are you sure you want to revert changes to the game settings?",
                "revert",
                "cancel");
        }

        void ResetSettings()
        {
            ConfirmWindow.window.Open(
                () => 
                {
                    Settings.ResetSettings();
                    LoadFromSettings(Settings.GetData()); 
                },
                "Reset settings?",
                "Are you sure you want to reset ALL changes to the game settings?",
                "reset",
                "cancel");
        }
    }
}
