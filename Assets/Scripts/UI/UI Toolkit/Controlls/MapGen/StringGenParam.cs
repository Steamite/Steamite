using StartMenu;
using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Params
{
    /// <summary>Text input field parameter.</summary>
    [UxmlElement]
    public partial class StringGenerationParameter : BindableElement
    {
        #region Variables
        /// <summary>Text value.</summary>
        string val;
        /// <summary>Label text value.</summary>
        string _lText;
        /// <summary>Label for identifying.</summary>
        protected Label label;
        #endregion

        #region Properties
        /// <inheritdoc cref="val"/>
        [CreateProperty]
        public string TextValue
        {
            get { return val; }
            set { 
                val = value;
                Debug.Log(value);
            }
        }

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
        #endregion

        #region Construction
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
        #endregion

        /// <summary>
        /// Links to seed.
        /// </summary>
        /// <param name="mapGeneration">Data source.</param>
        /// <returns>New seed.</returns>
        public string Link(MapGeneration mapGeneration, string displayName)
        {
            label.text = displayName;
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

        /// <summary>
        /// Is updated from outside.
        /// </summary>
        /// <param name="newStr">New value.</param>
        public void Change(string newStr)
        {
            this.Q<TextField>().SetValueWithoutNotify(newStr);
        }
    }
}
