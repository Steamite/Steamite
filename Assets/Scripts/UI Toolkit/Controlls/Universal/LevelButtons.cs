using AbstractControls;
using System.Collections.Generic;
using UnityEngine.UIElements;

[UxmlElement]
public partial class LevelButtons : CustomRadioButtonGroup
{
    [UxmlAttribute] List<int> levels = new() { 1, 2, 3, 4, 5 };
    public LevelButtons() : base()
    {
        buttons = new();
        for (int i = 0; i < levels?.Count; i++)
        {
            CustomRadioButton button = new("status-bar-button", i, this);
            button.text = $"{levels[i]}";
            button.style.marginTop = 5;
            button.style.marginBottom = 5;
            button.enabledSelf = false;
            Add(button);
        }
    }

    public void Start()
    {
        SelectedChoice = 0;
        SetChangeCallback((i) => MyGrid.ChangeGridLevel(i));
        buttons[0].SelectWithoutTransition(false);
        MyGrid.AddToGridChange(OutsideTrigger);
        object level;
        for (int i = 0; i < levels.Count; i++)
        {
            level = MyGrid.GetGroundLevelData(i);
            hierarchy[i].SetBinding(
                nameof(GroundLevel.Unlocked),
                nameof(enabledSelf),
                dataSource: level);
        }
    }

    public void OutsideTrigger(int old, int newI)
    {
        buttons[old].Deselect(false);
        SelectedChoice = newI;
        buttons[newI].Transition();
    }
}