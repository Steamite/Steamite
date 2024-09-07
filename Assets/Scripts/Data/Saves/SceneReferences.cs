using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneReferences : MonoBehaviour
{
    public Transform eventSystem;
    public Transform pipes;
    public Transform buildings;
    public Transform rocks;
    public Transform roads;
    public Transform chunks;
    public Transform water;
    public Transform humans;
    public Transform research;
    public Transform levelCamera;
    public Transform timeButtons;
    public UIOverlay OverlayCanvas;

    //public GameObject

    public void Clear()
    {
        Destroy(this);
    }
}