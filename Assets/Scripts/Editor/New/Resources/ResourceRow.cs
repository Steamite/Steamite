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
        DataAssignRow<ResourceData, ResourceTypeCategory, ResourceType> assignField;
        IntegerField integerField;
        
        public ResourceRow()
        {
            style.flexDirection = FlexDirection.Row;
            Add(assignField = new()
            {
            });
            Add(integerField = new()
            {
                label = "",
                maxLength = 3,
            });
        }

        public void Bind(SerializedProperty resource)
        {
            assignField.Bind(ResFluidTypes.Data, resource.FindPropertyRelative(nameof(ResourceNew.assign)));
            integerField.BindProperty(resource.FindPropertyRelative(nameof(ResourceNew.amount)));
        }
    }
}
