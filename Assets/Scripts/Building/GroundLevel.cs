using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GroundLevel : MonoBehaviour
{
    int activeLevel;
    public void SetLevel(int i)
    {
        transform.GetChild(activeLevel - 1).GetComponent<Button>().interactable = false;
        activeLevel = i;
        print($"active level is: {activeLevel}");
    }
}
