
using EditorWindows.Windows;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.LightingExplorerTableColumn;

public class ResourceRegister : DataGridWindow<ResourceTypeCategory, ResourceWrapper>
{
    //[MenuItem("Custom Editors/Resource register %n", priority = 14)]
    public static void Open()
    {
        return;
        ResourceRegister wnd = GetWindow<ResourceRegister>();
        wnd.titleContent = new("Resource Register");
        wnd.minSize = new(800, 400);
    }
    protected override void CreateGUI()
    {
        return;
        holder = AssetDatabase.LoadAssetAtPath<ResourceData>("Assets/Game Data/ResourceData.asset");
        base.CreateGUI();
        rootVisualElement.Q<Button>("Rebind-Create").clicked += async () => await ResFluidTypes.Init();
        categorySelector.index = 0;
    }

    protected override bool LoadCategData(int index)
    {
        bool boo = base.LoadCategData(index);
        if (boo)
        {
        }
        else
        {
            selectedCategory = new ResourceTypeCategory();
            selectedCategory.Objects = new();
        }
        return boo;
    }
    protected override void AddEntry(BaseListView _)
    {
        selectedCategory.Objects.Add(new ResourceWrapper(holder.UniqueID()));
        base.AddEntry(_);
    }

    protected override void CreateColumns()
    {
        base.CreateColumns();
        dataGrid.columns["asset"].makeCell =
                () => new ObjectField();
        dataGrid.columns["asset"].bindCell =
            (el, i) =>
            {
                ObjectField field = (ObjectField)el;
                field.allowSceneObjects = false;
                field.objectType = typeof(ResourceType);
                field.value = ((ResourceWrapper)dataGrid.itemsSource[i]).data;
                field.RegisterValueChangedCallback(AssetChange);
            };
        dataGrid.columns["asset"].unbindCell =
            (el, i) =>
            {
                ObjectField field = (ObjectField)el;
                field.UnregisterValueChangedCallback(AssetChange);
            };

        dataGrid.columns.Add(new()
        {
            title = "Color",
            resizable = true,
            minWidth = 200,
            makeCell = () => new ColorField(),
            bindCell = (el, i) =>
            {
                ColorField field = (ColorField)el;
                if (((ResourceWrapper)dataGrid.itemsSource[i]).data == null)
                {
                    field.style.display = DisplayStyle.None;
                    return;
                }
                field.style.display = DisplayStyle.Flex;

                field.value = ((ResourceWrapper)dataGrid.itemsSource[i]).data.color;
                field.RegisterValueChangedCallback(ColorChange);
            },
            unbindCell = (el, i) =>
            {
                ColorField field = (ColorField)el;
                field.UnregisterValueChangedCallback(ColorChange);
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
                field.objectType = typeof(Texture2D);
                if (((ResourceWrapper)dataGrid.itemsSource[i]).data == null)
                {
                    field.style.display = DisplayStyle.None;
                    return;
                }
                field.style.display = DisplayStyle.Flex;
                field.value = ((ResourceWrapper)dataGrid.itemsSource[i]).data.image;
                field.RegisterValueChangedCallback(ImageChange);
            },
            unbindCell = (el, i) =>
            {
                ObjectField field = (ObjectField)el;
                field.UnregisterValueChangedCallback(ImageChange);
            }

        });
    }

    private void ImageChange(ChangeEvent<UnityEngine.Object> evt)
    {
        int i = evt.target.GetRowIndex();
        if (i != -1)
        {
            ((ResourceWrapper)dataGrid.itemsSource[i]).data.image = evt.newValue as Texture2D;
            EditorUtility.SetDirty(holder);
            dataGrid.RefreshItem(i);
        }
    }

    private void ColorChange(ChangeEvent<Color> evt)
    {
        int i = evt.target.GetRowIndex();
        if (i != -1)
        {
            ((ResourceWrapper)dataGrid.itemsSource[i]).data.color = evt.newValue;
            EditorUtility.SetDirty(holder);
            dataGrid.RefreshItem(i);
        }
    }



    private void AssetChange(ChangeEvent<UnityEngine.Object> ev)
    {
        int i = ev.target.GetRowIndex();
        if (holder.Categories.SelectMany(q => q.Objects).Select(q=> q.data).Contains(ev.newValue) == false)
        {
            ((ResourceWrapper)dataGrid.itemsSource[i]).data = ev.newValue as ResourceType;
            ((ResourceWrapper)dataGrid.itemsSource[i]).Name = (ev.newValue as ResourceType)?.Name;
            EditorUtility.SetDirty(holder);

            dataGrid.RefreshItem(i);
        }
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
        if (((ResourceWrapper)dataGrid.itemsSource[i]).Name != value)
        {
            ((ResourceWrapper)dataGrid.itemsSource[i]).Name = value;
            EditorUtility.SetDirty(holder);

            ((ResourceWrapper)dataGrid.itemsSource[i]).data.Name = value;
            EditorUtility.SetDirty(((ResourceWrapper)dataGrid.itemsSource[i]).data);
        }
    }
}

