using Newtonsoft.Json;
using Settings.Sound;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Analytics.IAnalytic;

namespace Settings
{
    public class Settings : MonoBehaviour
    {
        [SerializeField] SettingsData defaultSettings;
        [SerializeField] SettingsData settings;

        string Path => System.IO.Path.Combine(Application.persistentDataPath, SETTINGS_PATH);
        const string SETTINGS_PATH = "settings.json";
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
            if (!File.Exists(Path))
            {
                DoReset();
                return;
            }

            JsonSerializer jsonSerializer = SaveController.PrepSerializer();
            using(StreamReader reader = new StreamReader(Path))
            {
                using JsonTextReader jsonReader = new(reader);
                settings = jsonSerializer.Deserialize<SettingsData>(jsonReader);
            }
            ApplySettings();
        }

        public static SettingsData GetData() => instance.settings;
        public static void ResetSettings() => instance.DoReset();
        public static void TestSettings(SettingsData data) => instance.DoTest(data);

        void DoTest(SettingsData data)
        {
            SettingsData oldSettings = instance.settings;
            settings = data;
            ApplySettings();
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

            SaveToDisk();
        }

        void SaveToDisk()
        {
            JsonSerializer jsonSerializer = SaveController.PrepSerializer();
            using StreamWriter writer = new StreamWriter(Path);
            using JsonTextWriter jsonWriter = new(writer);
            jsonSerializer.Serialize(jsonWriter, settings);
        }


        void DoReset()
        {
            settings = defaultSettings;
            var res = Screen.currentResolution;
            settings.Width = res.width;
            settings.Height = res.height;
            settings.MaxFPS = (int)Math.Round(res.refreshRateRatio.value);
            ApplySettings();
        }
    }
}
