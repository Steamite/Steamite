using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public enum InfoMode
{
    Building,
    Human,
    Rock,
    Chunk,
    Water,
    Research,
}

public class InfoWindow : MonoBehaviour
{
    public Transform cTransform;

    [SerializeField] public Transform clickObjectTransform;
    [SerializeField] public Transform researchTransform;
    [SerializeField] TMP_Text header;
    public Elevator selectedTabObject;

    public int currentTab;

    int activeInfo = -1;

    public void SwitchMods(InfoMode active, string _header)
    {
        header.text = _header;
        if ((int)active != activeInfo)
        {
            if ((InfoMode)activeInfo < InfoMode.Research && activeInfo > -1)
            {
                clickObjectTransform.gameObject.SetActive(false);
                clickObjectTransform.GetChild((int)activeInfo).gameObject.SetActive(false);
            }
            else
                researchTransform.gameObject.SetActive(false);

            if(active < InfoMode.Research)
            {
                clickObjectTransform.gameObject.SetActive(true);
                clickObjectTransform.GetChild((int)active).gameObject.SetActive(true);
            }
            else
                researchTransform.gameObject.SetActive(true);

            activeInfo = (int)active;
        }
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

    public void SetTab(int tab)
    {
        currentTab = tab;
        selectedTabObject.OpenWindow();
    }
}