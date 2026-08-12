using AbstractControls;
using System;
using System.Collections.Generic;
using System.Text;

public abstract class ShortcutRadioButtonGroup : CustomRadioButtonGroup
{
    public virtual void OutsideTrigger(int newI)
    {
        if(SelectedChoice > -1)
            buttons[SelectedChoice].Deselect(true);
        
        SelectedChoice = newI;
        
        if(newI > -1)
            buttons[newI].Transition();
    }
}