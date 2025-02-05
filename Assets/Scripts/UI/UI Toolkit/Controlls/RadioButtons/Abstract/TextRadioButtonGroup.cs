using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace AbstractControls
{
    public class TextRadioButtonGroup : CustomRadioButtonGroup
    {
        string[] _choices = { };
        [UxmlAttribute]
        protected string[] choices
        {
            get => _choices;
            set
            {
                _choices = value;
                Rebuild();
            }
        }
        void Rebuild()
        {
            for (int i = 0; i < childCount; i++)
            {
                BindButton((TextRadioButton)ElementAt(i), i);
            }
            if (childCount < choices.Length)
            {
                for (int i = childCount; i < choices.Length; i++)
                {
                    TextRadioButton button = CreateButton(i);
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
        protected virtual void BindButton(TextRadioButton button, int i) => button.text = choices[i].ToString();
        protected virtual TextRadioButton CreateButton(int i) => new TextRadioButton("", i, true, choices[i].ToString());
    }
}
