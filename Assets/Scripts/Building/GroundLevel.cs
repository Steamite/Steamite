using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GroundLevel : MonoBehaviour
{
    public Transform rocks;
    public Transform roads;
    public Transform water;
    public Transform chunks;
    public Transform buildings;
    public Transform pipes;

    /*int activeLevel;
    public void SetLevel(int i)
    {
        transform.GetChild(activeLevel - 1).GetComponent<Button>().interactable = false;
        activeLevel = i;
        print($"active level is: {activeLevel}");
    }*/
}
