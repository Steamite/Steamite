using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class LevelButtons : RadioButtons
{
    public void Init()
    {
        /*for(int i = 0; i < MyGrid.NUMBER_OF_LEVELS; i++)
        {
            transform.GetChild(1 + i).GetComponent<Button>().interactable = MyGrid.IsUnlocked(i);
        }*/
    }

    protected override void ButtonTrigger(Button button, int index)
    {
        if (currentState == index)
        {
            return;
        }
        base.ButtonTrigger(button, index);
        MyGrid.ChangeGridLevel(states[index]);
    }
}
