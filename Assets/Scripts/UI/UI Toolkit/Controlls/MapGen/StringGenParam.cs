using StartMenu;
using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Params
{
    [UxmlElement]
    public partial class StringGenerationParameter : BindableElement
    {
        string val;
        [CreateProperty]
        public string TextValue
        {
            get { return val; }
            set { 
                val = value;
                Debug.Log(value);
            }
        }

        string _lText;
        [UxmlAttribute]
        public string labelText
        {
            get { return _lText; }
            set
            {
                _lText = value;
                label.text = _lText;
            }
        }
        protected Label label;

        public StringGenerationParameter()
        {
            style.height = new(new Length(50, LengthUnit.Pixel));
            style.flexGrow = 1;
            style.flexDirection = FlexDirection.Row;

            label = new Label("Label");
            label.style.flexGrow = 1;
            label.focusable = false;
            Add(label);

            TextField textField = new TextField();
            textField.maxLength = 8;
            textField.style.flexGrow = 1;
            textField.style.maxWidth = 150;
            textField.RegisterValueChangedCallback<string>((str) => 
            {
                TextValue = str.newValue;
                NotifyPropertyChanged(nameof(TextValue));
            });

            Add(textField);
            VisualElement el = textField.Q<VisualElement>("unity-text-input");
            el.AddToClassList("param-text-field");
        }

        public string Link(MapGeneration mapGeneration)
        {
            dataSource = mapGeneration;
            string _seed = "";

            // Enum values
            for (int i = 0; i < 4; i++)
            {
                int r = (UnityEngine.Random.Range(0, 5) * 3) + 1;
                if(r > 9)
                {
                    _seed += (char)(r + 55);
                }
                else
                    _seed += r;
            }

            // Random seed
            for (int i = 0; i < 4; i++)
            {
                int random = UnityEngine.Random.Range(0, 16);
                if (random > 9)
                {
                    char c = (char)(random + 55);
                    _seed += c;
                }
                else
                    _seed += random;
            }

            Change(_seed);
            return _seed;
        }

        public void Change(string newStr)
        {
            this.Q<TextField>().SetValueWithoutNotify(newStr);
        }
    }
}
