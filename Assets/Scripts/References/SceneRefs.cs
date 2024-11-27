using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneRefs : MonoBehaviour
{
    static SceneRefs instance;
    [SerializeField] GridTiles _gridTiles;
    [SerializeField] ClickabeObjectFactory _objectFactory;
    [SerializeField] Humans _humans;
    [SerializeField] Tick _tick;
    [SerializeField] SaveController _controller;

    public static GridTiles gridTiles => instance._gridTiles;
    public static ClickabeObjectFactory objectFactory => instance._objectFactory;
    public static Humans humans => instance._humans;
    public static Tick tick => instance._tick;
    public static SaveController saveController => instance._controller;

    public void Init()
    {
        instance = this;
    }

    public void Clear()
    {
        Destroy(this);
    }
}