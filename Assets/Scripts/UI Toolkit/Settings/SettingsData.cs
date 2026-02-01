using System;

namespace Settings
{
    [Serializable]
    public class SettingsData
    {
        public int MusicVolume = 100;// { get; set; }
        public int EffectVolume = 100;// { get; set; }

        public int Width = 1920;// { get; set; }
        public int Height = 1080;// { get; set; }
        public bool Fullscreen = true;// { get; set; }
        public int MaxFPS = 60;// { get; set; }

        public int QualityLevel = 1;// { get; set; }
    }
}
