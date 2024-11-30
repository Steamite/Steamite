using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Params
{
    public class StringGenerationParameter : BindableElement
    {
        string val;
        [CreateProperty]
        public string TextValue
        {
            get { return val; }
            set { 
                val = value;
            }
        }

        string _lText;
        [CreateProperty]
        public string labelText
        {
            get { return _lText; }
            set
            {
                _lText = value;
                label.text = _lText;
                //ElementAt(1).ElementAt(0).Q<TextElement>().text = _lText;
            }
        }

        protected Label label;

        [Serializable]
        public new class UxmlSerializedData : BindableElement.UxmlSerializedData
        {
            [SerializeField]
            private string TextValue;
            [SerializeField]
            [HideInInspector]
            [UxmlIgnore]
            private UxmlAttributeFlags TextValue_UxmlAttributeFlags;

            [SerializeField]
            private string labelText;
            [SerializeField]
            [HideInInspector]
            [UxmlIgnore]
            private UxmlAttributeFlags labelText_UxmlAttributeFlags;

            public override object CreateInstance()
            {
                return new StringGenerationParameter();
            }

            public override void Deserialize(object obj)
            {
                base.Deserialize(obj);
                StringGenerationParameter generationParams = (StringGenerationParameter)obj;
                if (UnityEngine.UIElements.UxmlSerializedData.ShouldWriteAttributeValue(TextValue_UxmlAttributeFlags))
                {
                    generationParams.TextValue = TextValue;
                }
                /*if (UnityEngine.UIElements.UxmlSerializedData.ShouldWriteAttributeValue(seed_UxmlAttributeFlags))
                {
                    generationParams.bindingPath = bindingPath;
                }*/
                if (UnityEngine.UIElements.UxmlSerializedData.ShouldWriteAttributeValue(labelText_UxmlAttributeFlags))
                {
                    generationParams.labelText = labelText;
                }
            }
        }

        [System.Obsolete]
        public new class UxmlFactory : UxmlFactory<StringGenerationParameter, UxmlTraits> { }

        public new class UxmlTraits : BindableElement.UxmlTraits
        {
            // Bindable attributes
            private UxmlStringAttributeDescription TextValue = new UxmlStringAttributeDescription
            {
                name = "TextValue",
                defaultValue = "default"
            };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var control = (StringGenerationParameter)ve;
                control.TextValue = TextValue.GetValueFromBag(bag, cc);
            }
        }

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

            this.Q<TextField>().value = newStr;
            /*TextValue = newStr;
            NotifyPropertyChanged(nameof(TextValue));*/
        }
    }
}
