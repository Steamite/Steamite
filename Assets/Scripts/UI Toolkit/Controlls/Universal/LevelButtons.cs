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
        GridWorld world = SceneRefs.World;

        buttons = new();
        for (int i = 0; i < world.LEVEL_COUNT; i++)
        {
            CustomRadioButton button = new("status-bar-button", i, this);
            button.text = $"{i + 1}";
            button.style.marginTop = 5;
            button.style.marginBottom = 5;
            button.enabledSelf = false;
            Add(button);
        }


        SelectedChoice = world.CurrentLevelIndex;
        buttons[SelectedChoice].SelectWithoutTransition(false);

        SetChangeCallback((i) => world.SetLevel(i));


        world.AttachChange((_, newLevel) => OutsideTrigger(newLevel));


        object level;
        for (int i = 0; i < world.LEVEL_COUNT; i++)
        {
            level = world.GetLevel(i);
            hierarchy[i].SetBinding(
                nameof(GridLevel.Unlocked),
                nameof(enabledSelf),
                dataSource: level);
        }
    }
}