using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.Experimental.AI;
using UnityEngine.UIElements;

namespace Assets.Scripts.Editor.New.Resources
{
    public class ResourceRow : BindableElement
    {
        DropdownField categoryField;
        DropdownField objectField;
        IntegerField integerField;
        
        public ResourceRow()
        {
            style.flexDirection = FlexDirection.Row;

            Add(categoryField = new()
            {
            });
            Add(objectField = new()
            {
            });
            Add(integerField = new()
            {
                label = "",
                maxLength = 3,
            });
        }

        public void Bind()
        {

        }
    }
}
