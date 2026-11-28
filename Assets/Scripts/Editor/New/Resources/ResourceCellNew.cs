using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Assets.Scripts.Editor.New.Resources
{
    public class ResourceCellNew : BindableElement
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
                bindingPath = nameof(MoneyNew.resource)
            });

            Add(cost = new() 
            { 
                bindingPath = nameof(MoneyNew.moneyCost)
            });
        }
    }
}

