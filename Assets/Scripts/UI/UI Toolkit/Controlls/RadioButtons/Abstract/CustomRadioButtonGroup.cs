using System;
using UnityEngine.UIElements;

namespace AbstractControls
{
    [UxmlElement]
    public partial class CustomRadioButtonGroup : VisualElement
    {
        protected event Action<int> changeEvent;

        public int SelectedChoice
        {
            get;
            protected set;
        }


        string[] _choices = { "1", "2" };
        [UxmlAttribute] protected string[] choices 
        { 
            get => _choices; 
            set 
            { 
                _choices = value;
                Rebuild();
            } 
        }
        public CustomRadioButtonGroup()
        {
            SelectedChoice = -1;
            style.flexGrow = 1;
            style.justifyContent = Justify.SpaceAround;
        }

        void Rebuild()
        {
            for (int i = 0; i < childCount; i++)
            {
                BindButton((CustomRadioButton)ElementAt(i), i);
            }
            if(childCount < choices.Length)
            {
                for (int i = childCount; i < choices.Length; i++)
                {
                    CustomRadioButton button = CreateButton(i);
                    var x = i;
                    button.RegisterCallback<ClickEvent>((_) => Select(x));
                    Add(button);
                }
            }
            else if (childCount > choices.Length)
            {
                for (int i = childCount; i >= choices.Length; i--)
                {
                    RemoveAt(i);
                }
            }
        }
        protected virtual void BindButton(CustomRadioButton button, int i) => button.text = choices[i];
        protected virtual CustomRadioButton CreateButton(int i) => new CustomRadioButton(choices[i], "", i, true);

        public virtual void SetChangeCallback(Action<int> onChange)
        {
            changeEvent = onChange;
        }

        public virtual void Select(int value)
        {
            if (SelectedChoice > -1 && SelectedChoice != value)
                ((CustomRadioButton)ElementAt(SelectedChoice)).Deselect();
            SelectedChoice = value;
            changeEvent?.Invoke(value);
        }
    }
}