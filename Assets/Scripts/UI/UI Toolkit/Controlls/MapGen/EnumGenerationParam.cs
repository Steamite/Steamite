using StartMenu;
using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Params
{
    enum GenParamEnum
    {
        Sparse,
        Medium,
        Abundant
    }

    public class EnumGenerationParameter : BindableElement
    {
        protected int val;
        [CreateProperty]
        public int IntValue
        {
            get { return val; }
            set { 
                val = value;
            }
        }

        protected string _lText;
        [CreateProperty]
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

        [Serializable]
        public new class UxmlSerializedData : BindableElement.UxmlSerializedData
        {
            [SerializeField]
            private int IntValue;
            [SerializeField]
            [HideInInspector]
            [UxmlIgnore]
            private UxmlAttributeFlags IntValue_UxmlAttributeFlags;

            [SerializeField]
            private string labelText;
            [SerializeField]
            [HideInInspector]
            [UxmlIgnore]
            private UxmlAttributeFlags labelText_UxmlAttributeFlags;

            public override object CreateInstance()
            {
                return new EnumGenerationParameter();
            }

            public override void Deserialize(object obj)
            {
                base.Deserialize(obj);
                EnumGenerationParameter generationParams = (EnumGenerationParameter)obj;
                if (UnityEngine.UIElements.UxmlSerializedData.ShouldWriteAttributeValue(IntValue_UxmlAttributeFlags))
                {
                    generationParams.IntValue = IntValue;
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
        public new class UxmlFactory : UxmlFactory<EnumGenerationParameter, UxmlTraits> { }

        [Obsolete]
        public new class UxmlTraits : BindableElement.UxmlTraits
        {
            // Bindable attributes
            private UxmlIntAttributeDescription IntValue = new UxmlIntAttributeDescription
            {
                name = "IntValue",
                defaultValue = 0
            };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var control = (EnumGenerationParameter)ve;
                control.IntValue = IntValue.GetValueFromBag(bag, cc);
            }
        }

        public EnumGenerationParameter()
        {
            style.height = new(new Length(50, LengthUnit.Pixel));
            style.unityTextAlign = TextAnchor.MiddleLeft;
            style.fontSize = 37;
            style.marginTop = 10;

            EnumField enumField = new EnumField("Label", GenParamEnum.Medium);
            enumField.ElementAt(1).AddToClassList("enum-style");
            enumField.RegisterCallback<ChangeEvent<System.Enum>>(test);
            enumField.focusable = false;
            enumField.AddToClassList("Empty");


            label = enumField.labelElement;
            label.focusable = false;
            label.AddToClassList("Empty");
            enumField.style.justifyContent = Justify.SpaceBetween;

            Add(enumField);
            NotifyPropertyChanged(nameof(IntValue));
        }

        public void test(ChangeEvent<System.Enum> change)
        {
            GenParamEnum am = (GenParamEnum)change.newValue;
            IntValue = (int)am;
            NotifyPropertyChanged(nameof(IntValue));
        }

        public int Link(MapGeneration mapGeneration)
        {
            dataSource = mapGeneration;
            return 1;
        }

        public void Change(int i)
        {
            this.Q<EnumField>().SetValueWithoutNotify((GenParamEnum)i);
        }
    }
}
