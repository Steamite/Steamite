using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using static UnityEngine.Analytics.IAnalytic;

namespace Assets.Scripts.Editor.New.Resources
{
    public class ResourceCellNew : VisualElement
    {
        ListView view;
        IntegerField cost;
        public ResourceCellNew()
        {
            Add(view = new()
            {
                makeItem = () =>
                {
                    return new ResourceRow();
                },
                bindItem = (el, i) =>
                {
                    (el as ResourceRow).Bind(resourceProp.GetArrayElementAtIndex(i));
                },/*
                allowAdd = true,
                allowRemove = true,
                showBoundCollectionSize = true*/
                //bindingPath = nameof(MoneyNew.resource)
            });

            Add(cost = new() 
            { 
              //  bindingPath = nameof(MoneyNew.moneyCost)
            });
            
        }

        SerializedProperty resourceProp;

        public void Bind(SerializedProperty property)
        {
            resourceProp = property.FindPropertyRelative(nameof(MoneyNew.resource));
            view.BindProperty(resourceProp);
            cost.BindProperty(property.FindPropertyRelative(nameof(MoneyNew.moneyCost)));
        }
    }
}

