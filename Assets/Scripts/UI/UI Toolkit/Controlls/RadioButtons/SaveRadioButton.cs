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
            style.unityTextAlign = TextAnchor.MiddleLeft;
            style.paddingLeft = new(new Length(5, LengthUnit.Percent));

            saveDate = new TextElement();
            saveDate.text = date.ToString();
            saveDate.style.alignSelf = Align.FlexEnd;
            saveDate.style.unityTextAlign = TextAnchor.LowerRight;
            style.justifyContent = Justify.FlexEnd;
            this.Add(saveDate);

            deleteButton = new Button(() => parent.parent.parent.Q<SaveRadioGroup>()?.DeleteSave());
            deleteButton.AddToClassList("delete-button");
            deleteButton.style.display = DisplayStyle.None;
            this.Add(deleteButton);
        }

        public override void Select(ClickEvent _)
        {
            base.Select(_);
            deleteButton.style.display = DisplayStyle.Flex;
        }
        public override void Deselect()
        {
            base.Deselect();
            deleteButton.style.display = DisplayStyle.None;
        }
    }
}