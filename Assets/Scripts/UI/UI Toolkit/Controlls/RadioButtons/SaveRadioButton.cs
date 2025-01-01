using AbstractControls;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace RadioGroups
{
    public class SaveRadioButton : CustomRadioButton
    {
        public TextElement saveDate;
        public Button deleteButton;

        [System.Obsolete]
        public new class UxmlFactory : UxmlFactory<CustomRadioButton, UxmlTraits> { }
        public SaveRadioButton() : base()
        {
        }

        public SaveRadioButton(string labelText, string _styleClass, int i, DateTime date) : base(labelText, _styleClass, i)
        {
            saveDate = new TextElement();
            saveDate.text = date.ToString();
            saveDate.style.alignSelf = Align.FlexEnd;
            saveDate.style.unityTextAlign = TextAnchor.LowerRight;
            saveDate.style.fontSize = 30;
            style.justifyContent = Justify.FlexEnd;
            this.Add(saveDate);

            deleteButton = new Button(() => parent.parent.parent.Q<SaveRadioGroup>()?.DeleteSave());
            deleteButton.AddToClassList("delete-button");
            deleteButton.style.display = DisplayStyle.None;
            this.Add(deleteButton);
        }

        public override void Select(bool triggerTransition = true)
        {
            base.Select(triggerTransition);
            deleteButton.style.display = DisplayStyle.Flex;
        }
        public override void Deselect(bool triggerTransition = true)
        {
            base.Deselect(triggerTransition);
            deleteButton.style.display = DisplayStyle.None;
        }
    }
}