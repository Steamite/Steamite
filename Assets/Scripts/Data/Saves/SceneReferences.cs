using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneReferences : MonoBehaviour
{
    public Transform eventSystem;
    public Transform humans;
    public Transform levelCamera;
    public List<GroundLevel> levels;

    //public GameObject

    public void Clear()
    {
        Destroy(this);
    }
}