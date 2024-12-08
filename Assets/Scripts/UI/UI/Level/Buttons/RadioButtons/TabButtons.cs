using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabButtons : RadioButtons
{
    [SerializeField] List<GameObject> tabs;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void SelectButton(Button button)
    {
        tabs[states[previusState]].SetActive(false);
        base.SelectButton(button);
        tabs[states[currentState]].SetActive(true);
    }
}
