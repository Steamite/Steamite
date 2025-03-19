using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

abstract public class RadioButtons : MonoBehaviour
{
    [SerializeField] protected List<int> states = new() {};
    [SerializeField] protected int previusState = 0;
    [SerializeField] protected int currentState = 1;


    protected Button lastAccesedButton;

    protected virtual void Awake()
    {
        int i = 0;
        foreach (Button button in transform.GetComponentsInChildren<Button>())
        {
            var x = i;
            var b = button;
            button.onClick.AddListener(() => ButtonTrigger(b, x));

            if (i == currentState)
            {
                SelectButton(b);
            }
            i++;
        }
    }

    virtual protected void SelectButton(Button button)
    {
        button.interactable = false;
        lastAccesedButton = button;
    }

    virtual protected void DeselectButton(Button button = null)
    {
        if (button == null)
            button = lastAccesedButton;
        button.interactable = true;
    }

    virtual protected void ButtonTrigger(Button button, int index)
    {
        DeselectButton();
        currentState = index;
        SelectButton(button);
        previusState = currentState;
    }

    public void OutsideTrigger(int index)
    {
        Button button = transform.GetComponentsInChildren<Button>()[index];
        ButtonTrigger(button, index);
    }
}
