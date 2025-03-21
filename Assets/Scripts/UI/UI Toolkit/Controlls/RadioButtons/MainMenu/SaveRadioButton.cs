using AbstractControls;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace RadioGroups
{/*
    public class SaveRadioButtonData : CustomRadioButtonData
    {
        public Button
    }
*/
    [UxmlElement]
    public partial class SaveRadioButton : TextRadioButton
    {
        public TextElement saveDate;
        public Button deleteButton;

        public SaveRadioButton() : base()
        {
        }

        public SaveRadioButton(string labelText, string _styleClass, int i, DateTime date) : base(_styleClass, i, false, labelText)
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

        protected override void SelectChange(bool UpdateGroup)
        {
            base.SelectChange(UpdateGroup);
            deleteButton.style.display = DisplayStyle.Flex;
        }
        public override void Deselect(bool triggerTransition = true)
        {
            base.Deselect(triggerTransition);
            deleteButton.style.display = DisplayStyle.None;
        }
        /*public override void Bind(CustomRadioButtonData buttonData)
        {
            base.Bind(buttonData);
        }*/
    }
}