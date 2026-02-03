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


        Button saveSettings;
        
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
                data.Width = int.Parse(q.newValue.Split('x')[0]);
                data.Height = int.Parse(q.newValue.Split('x')[1]);
                UpdateButtonState();
            });

            saveSettings = menu.Q<Button>("Save-Settings-Button");

            saveSettings.clicked += SaveSettings;

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
        }

        private void SaveSettings()
        {
            /*
            data = new() 
            { 
                MasterVolume = masterVolumeSlider.value,
                MusicVolume = musicVolumeSlider.value,
                EffectVolume = effectVolumeSlider.value,
                Mute = musicToggle.value,
                VSync = vsyncToggle.value,
                fullScreenMode = (FullScreenMode)screenSettings.IndexOf(screenDropdown.value),
                MaxFPS = int.Parse(fpsDropdown.value),
                Width = int.Parse(resolutionDropdown.value.Split('x')[0]),
                Height = int.Parse(resolutionDropdown.value.Split('x')[1])
            };*/

            Settings.TestSettings(data);
            UpdateButtonState();
        }

    }
}
