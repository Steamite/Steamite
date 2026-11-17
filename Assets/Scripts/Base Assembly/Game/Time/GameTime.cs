using Unity.Burst;
using UnityEngine;

public abstract class GameTime 
{
    private class DeltaKey { };
    private class ScaleKey { };
    static readonly SharedStatic<float> deltaTime = SharedStatic<float>.GetOrCreate<GameTime, DeltaKey>();
    static readonly SharedStatic<float> scaleTime = SharedStatic<float>.GetOrCreate<GameTime, ScaleKey>();

    public static float DeltaTime => deltaTime.Data;
    public static float ScaleTime => scaleTime.Data;

    public static void SetTimeScale(float value)
    {
        scaleTime.Data = value;
    }

    public static void SetDeltaTime(float value)
    {
        deltaTime.Data = value;
    }
}
