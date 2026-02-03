using Settings.Sound;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace Settings
{
    public class Settings : MonoBehaviour
    {
        [SerializeField] SettingsData settings;
        [SerializeField] string settingPath = "/settings.json";
        MusicPlayer player;

        static Settings instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void ClearInstance()
        {
            instance = null;
        }

        void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            player = transform.GetChild(0).GetComponent<MusicPlayer>();
            
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            string path = Application.persistentDataPath + settingPath;
            
            if (File.Exists(path))
            {
                JsonSerializer jsonSerializer = SaveController.PrepSerializer();
                using JsonTextReader jsonReader = new(new StreamReader(path));
                settings = jsonSerializer.Deserialize<SettingsData>(jsonReader);
            }

            ApplySettings();
        }

        public static SettingsData GetData() => instance.settings;
        public static void TestSettings(SettingsData data)
        {
            SettingsData oldSettings = instance.settings;
            instance.settings = data;
            instance.ApplySettings();
        }

        void ApplySettings()
        {
            Screen.SetResolution(settings.Width, settings.Height, settings.fullScreenMode);
            QualitySettings.SetQualityLevel(settings.QualityLevel);
            QualitySettings.vSyncCount = settings.VSync ? 1 : 0;
            if(settings.VSync == false)
                Application.targetFrameRate = settings.MaxFPS;
            else
                Application.targetFrameRate = -1;

            player.SetVolume(settings.Mute, settings.MasterVolume, settings.MusicVolume, settings.EffectVolume);
        }

    }
}
