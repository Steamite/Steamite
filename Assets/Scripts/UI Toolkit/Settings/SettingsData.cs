using System;
using UnityEngine;

namespace Settings
{

    [Serializable]
    public struct SettingsData
    {
        public bool Mute;// { get; set; }
        public int MasterVolume;// { get; set; }
        public int MusicVolume ;// { get; set; }
        public int EffectVolume;// { get; set; }

        public bool VSync;// { get; set; }
        public int MaxFPS;// { get; set; }
        public FullScreenMode fullScreenMode;// { get; set; }
        
        public int Width;// { get; set; }
        public int Height;// { get; set; }

        public int QualityLevel;// { get; set; }

        public override bool Equals(object obj)
        {
            SettingsData data = (SettingsData)obj;
            return Mute == data.Mute &&
                   MasterVolume == data.MasterVolume &&
                   MusicVolume == data.MusicVolume &&
                   EffectVolume == data.EffectVolume &&
                   VSync == data.VSync &&
                   MaxFPS == data.MaxFPS &&
                   fullScreenMode == data.fullScreenMode &&
                   Width == data.Width &&
                   Height == data.Height &&
                   QualityLevel == data.QualityLevel;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(Mute);
            hash.Add(MasterVolume);
            hash.Add(MusicVolume);
            hash.Add(EffectVolume);
            hash.Add(VSync);
            hash.Add(MaxFPS);
            hash.Add(fullScreenMode);
            hash.Add(Width);
            hash.Add(Height);
            hash.Add(QualityLevel);
            return hash.ToHashCode();
        }
    }
}
