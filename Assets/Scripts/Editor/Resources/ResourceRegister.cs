using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
public class ResourceRegister : DataGridWindow<ResourceTypeCategory, ResourceType>
{
    ColorField colorField;
    [MenuItem("Custom Editors/Resource register %n", priority = 14)]
    public static void Open()
    {
        ResourceRegister wnd = GetWindow<ResourceRegister>();
        wnd.titleContent = new("Resource Register");
        wnd.minSize = new(800, 400);
    }
    protected override void CreateGUI()
    {
        Holder = AssetDatabase.LoadAssetAtPath<ResourceData>(ResourceData.EDITOR_PATH);
        base.CreateGUI();
        colorField = rootVisualElement.Q<ColorField>();
        rootVisualElement.Q<Button>("Rebind-Create").clicked += () => ResFluidTypes.EditorInit();
        categorySelector.index = 0;
    }

    protected override bool LoadCategData(int index)
    {
        bool b = base.LoadCategData(index);
        if (b)
        {
            colorField.style.display = DisplayStyle.Flex;
            colorField.BindProperty(serializedCategory.FindPropertyRelative(nameof(ResourceTypeCategory.color)));
            bool a = false;
            for (int i = 0; i < SelectedCategory.Objects.Count; i++)
            {
                if (SelectedCategory.Objects[i] == null)
                {
                    SelectedCategory.Objects[i] = new ResourceType();
                    a = true;
                }
            }
            if(a)
                EditorUtility.SetDirty(holder);
        }
        else
        {
            colorField.Unbind();
            colorField.style.display = DisplayStyle.None;
        }

        return b;
    }

    protected override void TopBar(out ObjectField iconSelector)
    {
        base.TopBar(out iconSelector);
    }

    protected override void CreateColumns()
    {
        base.CreateColumns();
        dataGrid.columns.Add(new()
        {
            title = "Color",
            resizable = true,
            minWidth = 200,
            makeCell = () => new ColorField(),
            bindCell = (el, i) =>
            {
                ColorField field = (ColorField)el;
                field.BindProperty(GetWrapperAt(i).FindPropertyRelative(nameof(ResourceType.color)));
            },
            unbindCell = (el, i) =>
            {
                ColorField field = (ColorField)el;
                field.Unbind();
            }

        });


        dataGrid.columns.Add(new()
        {
            title = "Texture2D",
            resizable = true,
            minWidth = 200,
            makeCell = () => new ObjectField(),
            bindCell = (el, i) =>
            {
                ObjectField field = (ObjectField)el;
                field.BindProperty(GetWrapperAt(i).FindPropertyRelative(nameof(ResourceType.image)));
            },
            unbindCell = (el, i) =>
            {
                ObjectField field = (ObjectField)el;
                field.Unbind();
            }

        });
    }
}

