using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

public struct RadiusUtil
{
    readonly Vector2Int center;
    readonly int size;
    Action<int, int, int> action;

    public RadiusUtil(Vector2Int center, int size, Action<int, int, int> action)
    {
        this.center = center;
        this.size = size;
        this.action = action;
    }

    public void DoRadius()
    {
        for (int i = 1; i < size; i++)
        {
            DoLine(center.x, center.y + size - i, i);
            DoLine(center.x, center.y - size + i, i);
        }
        DoLine(center.x, center.y, size);
    }

    void DoLine(
        int x,
        int y,
        int valueOnCenter)
    {
        action.Invoke(x, y, valueOnCenter);
        int increaseVal;
        for (int i = 1; i < valueOnCenter; i++)
        {
            increaseVal = valueOnCenter - i;
            action.Invoke(x + i, y, valueOnCenter);
            action.Invoke(x - i, y, valueOnCenter);
        }
    }
}