using UnityEngine;
using UnityEngine.UIElements;

public class CustomRadioButton : Button
{

    public bool IsSelected { get; private set; }
    public string data;
    public int value;

    [System.Obsolete]
    public new class UxmlFactory : UxmlFactory<CustomRadioButton, UxmlTraits> { }

    public CustomRadioButton()
    {
        AddToClassList("radio-button");
    }

    public CustomRadioButton(string labelText, int i)
    {
        AddToClassList("radio-button");
        text = labelText;
        data = labelText;
        value = i;
    }

    public void Select(ClickEvent _)
    {
        if (IsSelected)
            return;
        IsSelected = true;
        style.backgroundColor = new(new Color(0.2862745f, 0.4745098f, 0.4196078f, 1));
        parent.parent.parent.parent.parent.parent.Q<CustomRadioButtonGroup>("Saves").Select(this);
    }

    public void Deselect()
    {
        IsSelected = false;
        style.backgroundColor = new(new Color(0.7372549f, 0.7372549f, 0.7372549f, 1));
    }
}
