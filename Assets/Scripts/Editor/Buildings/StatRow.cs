using BuildingStats;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.Editor.Buildings
{
    public class StatRow : VisualElement
    {
        public StatRow()
        {
            style.flexDirection = FlexDirection.Row;

            EnumField enumField = new EnumField(StatModifiers.Cost);
            enumField.style.flexGrow = 1;
            Add(enumField);

            FloatField floatField = new FloatField();
            floatField.style.width = 50;
            Add(floatField);

            Toggle toggle = new Toggle("%");
            toggle.style.flexDirection = FlexDirection.RowReverse;
            Label label = toggle.Q<Label>();
            label.style.paddingLeft = 10;
            label.style.minWidth = 40;
            label.style.maxWidth = 40;
            Add(toggle);
        }

        public void Open(SerializedProperty statValue)
        {
            EnumField enumField = this[0] as EnumField;
            SerializedProperty mod = statValue.FindPropertyRelative(nameof(StatValue.mod));
            enumField.BindProperty(mod);

            FloatField floatField = this[1] as FloatField;
            SerializedProperty modAmmount = statValue.FindPropertyRelative(nameof(StatValue.modAmmount));
            floatField.BindProperty(modAmmount);

            Toggle toggle = this[2] as Toggle;
            SerializedProperty percent = statValue.FindPropertyRelative(nameof(StatValue.percent));
            toggle.BindProperty(percent);
        }
    }
}
