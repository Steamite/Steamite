using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class InfoWindow : MonoBehaviour
{
    public Transform cTransform;
    int activeInfo = -1;
    public void SwitchMods(int active, string header)
    {
        if (active != activeInfo)
        {
            Transform t = transform.GetChild(1);
            transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = header;
            for (int i = 0; i < t.childCount; i++)
            {
                t.GetChild(i).gameObject.SetActive(i == active);
            }
            return;
        }
        return;
    }
    public void SetAssignButton(bool assign, Transform buttons) // toggles info worker to assigned and back
    {
        buttons.parent.GetChild(0).gameObject.SetActive(assign);
        buttons.GetChild(0).GetComponent<Button>().interactable = !assign;
        buttons.GetChild(0).GetChild(0).GetComponent<TMP_Text>().color = !assign ? Color.black : Color.white;
        buttons.GetChild(0).GetChild(0).GetComponent<TMP_Text>().fontStyle = !assign ? FontStyles.Normal : FontStyles.Bold;

        buttons.parent.GetChild(1).gameObject.SetActive(!assign);
        buttons.GetChild(1).GetComponent<Button>().interactable = assign;
        buttons.GetChild(1).GetChild(0).GetComponent<TMP_Text>().color = assign ? Color.black : Color.white;
        buttons.GetChild(1).GetChild(0).GetComponent<TMP_Text>().fontStyle = assign ? FontStyles.Normal : FontStyles.Bold;
    }
}