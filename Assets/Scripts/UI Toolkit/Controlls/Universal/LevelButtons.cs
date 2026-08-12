using AbstractControls;
using System.Collections.Generic;
using UnityEngine.UIElements;

[UxmlElement]
public partial class LevelButtons : ShortcutRadioButtonGroup, IInitiableUI
{
    public LevelButtons() : base()
    {
        
    }

    public void Init()
    {
        buttons = new();
        for (int i = 0; i < MyGrid.NUMBER_OF_LEVELS; i++)
        {
            CustomRadioButton button = new("status-bar-button", i, this);
            button.text = $"{i + 1}";
            button.style.marginTop = 5;
            button.style.marginBottom = 5;
            button.enabledSelf = false;
            Add(button);
        }


        SelectedChoice = MyGrid.currentLevel;
        buttons[SelectedChoice].SelectWithoutTransition(false);
        
        SetChangeCallback((i) => MyGrid.ChangeGridLevel(i));


        MyGrid.AddToGridChange((_, newLevel) => OutsideTrigger(newLevel));


        object level;
        for (int i = 0; i < MyGrid.NUMBER_OF_LEVELS; i++)
        {
            level = MyGrid.GetGroundLevelData(i);
            hierarchy[i].SetBinding(
                nameof(GroundLevel.Unlocked),
                nameof(enabledSelf),
                dataSource: level);
        }
    }
}