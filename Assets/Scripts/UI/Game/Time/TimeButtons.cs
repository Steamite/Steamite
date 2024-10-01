using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeButtons : MonoBehaviour
{
    public Tick tick;

    /// <summary>
    /// Called by speed buttons, manages transitions betwean them and changes the gamespeed.
    /// </summary>
    /// <param name="newSpeed"></param>
    public void SetSpeed(Button newSpeed)
    {
        if (!tick.lastButton)
        {
            tick.lastButton = newSpeed;
            transform.GetChild(3).GetComponent<Button>().interactable = true;
        }
        else
            tick.lastButton.interactable = true;
        newSpeed.interactable = false;
        tick.ChangeGameSpeed(int.Parse(newSpeed.name[0].ToString()));
        tick.lastButton = newSpeed;
    }
}
