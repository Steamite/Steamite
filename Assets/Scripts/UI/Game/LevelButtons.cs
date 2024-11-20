using UnityEngine.UI;

public class LevelButtons : RadioButtons
{
    protected override void ButtonTrigger(Button button, int index)
    {
        base.ButtonTrigger(button, index);
        MyGrid.ChangeGridLevel(states[index]);
    }
}
