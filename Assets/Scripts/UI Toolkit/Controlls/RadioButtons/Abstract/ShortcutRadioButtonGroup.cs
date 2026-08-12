using AbstractControls;
using System;
using System.Collections.Generic;
using System.Text;

public abstract class ShortcutRadioButtonGroup : CustomRadioButtonGroup
{
    public virtual void OutsideTrigger(int newI)
    {
        buttons[SelectedChoice].Deselect(true);
        SelectedChoice = newI;
        buttons[newI].Transition();
    }
}