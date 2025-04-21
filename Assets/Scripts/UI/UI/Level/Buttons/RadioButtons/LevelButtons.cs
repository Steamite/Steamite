using UnityEngine.UI;

public class LevelButtons : RadioButtons
{

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
