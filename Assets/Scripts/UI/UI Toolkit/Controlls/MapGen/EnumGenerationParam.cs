using StartMenu;
using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Params
{
    #region Enums
    /// <summary>3-state enum for map generation.</summary>
    public enum GenParamEnum
    {
        Sparse,
        Medium,
        Abundant
    }
    public enum MapSize
    {
        Small,
        Medium,
        Large
    }

    public enum InputEnum
    {
        MapSize,
        Veins
    }
    #endregion

    /// <summary>Enum field for map generation, Defaultly uses</summary>
    [UxmlElement]
    public partial class EnumGenerationParameter : BindableElement
    {
        #region Variables
        protected Label label;

        protected int val;
        protected string _lText;
        protected InputEnum inputEnum;
        #endregion

        #region Properties
        [UxmlAttribute][CreateProperty] public int IntValue
        {
            get { return val; }
            set
            {
                val = value;
            }
        }

        [UxmlAttribute] public string labelText
        {
            get { return _lText; }
            set
            {
                _lText = value;
                label.text = _lText;
            }
        }
        [UxmlAttribute] public InputEnum input
        {
            get { return inputEnum; }
            set
            {
                inputEnum = value;
                EnumType();
            }
        }
        #endregion
        
        #region Constructor
        public EnumGenerationParameter()
        {
            style.height = new(new Length(50, LengthUnit.Pixel));
            style.unityTextAlign = TextAnchor.MiddleLeft;
            style.fontSize = 37;
            style.marginTop = 10;

            EnumType();
            NotifyPropertyChanged(nameof(IntValue));
        }

        /// <summary>Creates a new enum field with the selected enum.</summary>
        void EnumType()
        {
            if (childCount > 0)
                RemoveAt(0);
            EnumField enumField = new EnumField("Label", GetEnumValue(1));
            enumField.ElementAt(1).AddToClassList("enum-style");
            enumField.RegisterCallback<ChangeEvent<Enum>>(EnumChange);
            enumField.focusable = false;
            enumField.AddToClassList("Empty");

            label = enumField.labelElement;
            label.focusable = false;
            label.AddToClassList("Empty");
            enumField.style.justifyContent = Justify.SpaceBetween;

            Add(enumField);
        }
        #endregion

        #region Enums conversion
        /// <summary>
        /// Converts int to the selected enum.
        /// </summary>
        /// <param name="x">Int value to convert.</param>
        /// <returns>Enum type with the <paramref name="x"/> value.</returns>
        Enum GetEnumValue(int x)
        {
            switch (inputEnum)
            {
                case InputEnum.MapSize:
                    return (MapSize)x;
                case InputEnum.Veins:
                    return (GenParamEnum)x;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Converts enum to int.
        /// </summary>
        /// <param name="e">Enum of selected type.</param>
        /// <returns>Enums value.</returns>
        int GetIntValue(Enum e)
        {
            switch (inputEnum)
            {
                case InputEnum.MapSize:
                    return (int)(MapSize)e;
                case InputEnum.Veins:
                    return (int)(GenParamEnum)e;
                default:
                    return -1;
            }
        }

        /// <summary>
        /// Enum change of user.
        /// </summary>
        /// <param name="change">New value.</param>
        public void EnumChange(ChangeEvent<Enum> change)
        {
            IntValue = GetIntValue(change.newValue);
            NotifyPropertyChanged(nameof(IntValue));
        }
        #endregion

        /// <summary>
        /// Links the value to the seed.
        /// </summary>
        /// <param name="mapGeneration">Map generation seed source.</param>
        /// <returns></returns>
        public int Link(MapGeneration mapGeneration, string displayName)
        {
            label.text = displayName;
            dataSource = mapGeneration;
            return IntValue;
        }

        /// <summary>
        /// Enum change from seed change.
        /// </summary>
        /// <param name="i">New value.</param>
        public void Change(int i)
        {
            this.Q<EnumField>().SetValueWithoutNotify((GenParamEnum)i);
        }
    }
}
