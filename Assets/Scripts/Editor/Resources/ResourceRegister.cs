using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        Debug.Log("Start load");
        bool b = base.LoadCategData(index);
        if (b)
        {
            colorField.value = SelectedCategory.color;
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
            colorField.value = new();
        }

        return b;
    }

    protected override void TopBar(out ObjectField iconSelector)
    {
        base.TopBar(out iconSelector);
        colorField.UnregisterValueChangedCallback(CategoryColorChange);
        colorField.RegisterValueChangedCallback(CategoryColorChange);
    }

    void CategoryColorChange(ChangeEvent<Color> ev)
    {
        if (SelectedCategory != null && ev.newValue != new Color())
        {
            SelectedCategory.color = ev.newValue;
            EditorUtility.SetDirty(Holder);
        }
    }

    protected override void CreateColumns()
    {
        //base.CreateColumns();
        /*dataGrid.columns["asset"].makeCell =
                () => new ObjectField();
        dataGrid.columns["asset"].bindCell =
            (el, i) =>
            {
                ObjectField field = (ObjectField)el;
                field.allowSceneObjects = false;
                field.objectType = typeof(ResourceTypeNew);
                field.value = ((ResourceWrapperNew)dataGrid.itemsSource[i]).data;
                field.RegisterValueChangedCallback(AssetChange);
            };
        dataGrid.columns["asset"].unbindCell =
            (el, i) =>
            {
                ObjectField field = (ObjectField)el;
                field.UnregisterValueChangedCallback(AssetChange);
            };*/

        dataGrid.columns.Add(new()
        {
            title = "Name",
            resizable = true,
            minWidth = 200,
            makeCell = () => new TextField(),
            bindCell = (el, i) =>
            {
                TextField field = (TextField)el;
                field.BindProperty(GetWrapperAt(i).FindPropertyRelative(nameof(ResourceType.Name)));
            },
            unbindCell = (el, i) =>
            {
                TextField field = (TextField)el;
                field.Unbind();
            }

        });

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

    protected override void NameChange(FocusOutEvent ev)
    {
        string value;
        if (ev.target is TextElement)
        {
            TextElement field = (TextElement)ev.target;
            value = field.text.Trim(' ');
            field.text = value;
        }
        else
        {
            TextField field = (TextField)ev.target;
            value = field.value.Trim(' ');
            field.value = value;
        }

        int i = ((VisualElement)ev.target).GetRowIndex();
        if (((ResourceType)dataGrid.itemsSource[i]).Name != value)
        {
            ((ResourceType)dataGrid.itemsSource[i]).Name = value;
            EditorUtility.SetDirty(Holder);
        }
    }
}

