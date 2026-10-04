using AbstractControls;
using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
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
        for (int i = 0; i < GridWorld.LEVEL_COUNT; i++)
        {
            CustomRadioButton button = new("status-bar-button", i, this);
            button.text = $"{i + 1}";
            button.style.marginTop = 5;
            button.style.marginBottom = 5;
            button.enabledSelf = false;
            Add(button);
        }


        SelectedChoice = GridWorld.CurrentLevelIndex;
        buttons[SelectedChoice].SelectWithoutTransition(false);

        SetChangeCallback((i) => GridWorld.SetLevel(i));


        GridWorld.AttachChange((_, newLevel) => OutsideTrigger(newLevel));


        object level;
        for (int i = 0; i < GridWorld.LEVEL_COUNT; i++)
        {
            level = GridWorld.GetLevel(i);
            hierarchy[i].SetBinding(
                nameof(GridLevel.Unlocked),
                nameof(enabledSelf),
                dataSource: level);
        }
    }
}