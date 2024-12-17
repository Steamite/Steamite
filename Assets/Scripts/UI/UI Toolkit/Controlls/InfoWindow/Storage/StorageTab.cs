using InfoWindowElements;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class StorageTab : Tab
{
    #region Elements
    struct StorageElem
    {
        public VisualElement icon;
        public Label label;
        public ToggleButtonGroup canStore;

        public StorageElem(VisualElement element)
        {
            icon = element.Q<VisualElement>("Icon");
            label = element.Q<Label>("Ammount");
            canStore = element.Q<ToggleButtonGroup>("Can-Store");
        }
    }

    public VisualTreeAsset elemPref;
    Label capacityLabel;
    ScrollView storageScroll;
    VisualElement lastRow;
    #endregion

    [CreateProperty]
    List<UIResource> resources;
    List<StorageElem> storageElems;

    Storage storage;

    public StorageTab() : base("Storage")
    {
        style.flexGrow = 1;
        name = "Storage";

        resources = new();
        storageElems = new();

        VisualElement element = new();
        element.AddToClassList("Capacity");
        capacityLabel = new("Capacity: ####/####");
        capacityLabel.AddToClassList("Capacity-Level");
        element.style.flexShrink = 0;
        element.Add(capacityLabel);
        Add(element);

        storageScroll = new ScrollView(ScrollViewMode.Vertical);
        storageScroll.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;
        storageScroll.style.minHeight = new(new Length(100, LengthUnit.Percent));
        Add(storageScroll);
    }

    public void Open(Storage _storage, VisualTreeAsset _elemPref)
    {
        storage = _storage;
        elemPref = _elemPref;
        DataBinding binding = SceneRefs.infoWindow.CreateBinding(nameof(Storage.LocalRes));
        binding.sourceToUiConverters.AddConverter((ref StorageResource store) => ToUIRes(store));
        SceneRefs.infoWindow.AddBinding(new(this, "resources"), binding, storage);

        binding = SceneRefs.infoWindow.CreateBinding(nameof(Storage.LocalRes));
        binding.sourceToUiConverters.AddConverter((ref StorageResource store) => $"Capacity: {store.stored.ammount.Sum()}/{store.stored.capacity}");
        SceneRefs.infoWindow.AddBinding(new(capacityLabel, "text"), binding, storage);

        if (storageElems.Count > 0)
        {
            for (int i = 0; i < storage.canStore.Count; i++)
            {
                UpdateGroup(storageElems[i], i);
            }
        }
    }

    #region Storage Managment
    List<UIResource> ToUIRes(StorageResource storage)
    {
        for (int i = 0; i < storage.stored.type.Count; i++)
        {
            if (i >= resources.Count)
            {
                if (i % 3 == 0)
                    AddNewRow();
                AddNewElem(new(storage.stored.ammount[i], storage.stored.type[i]));
            }
            else if (resources[i].ammount != storage.stored.ammount[i])
            {
                resources[i].ammount = storage.stored.ammount[i];
                storageElems[i].label.text = resources[i].ammount.ToString();
            }
        }
        return resources;
    }

    void ToggleCanStore(bool b, int i)
    {
        storage.canStore[i] = b;
    }
    void UpdateGroup(StorageElem storageElem, int x)
    {
        ToggleButtonGroupState state = storageElem.canStore.value;
        state.ResetAllOptions();
        storageElem.canStore.value = state;
        state[0] = storage.canStore[x];
        state[1] = !storage.canStore[x];
        storageElem.canStore.value = state;
    }

    #endregion

    #region New elements
    void AddNewElem(UIResource uiResource)
    {
        resources.Add(uiResource);
        VisualElement element = elemPref.CloneTree();
        StorageElem storageElem = new(element);
        var x = resources.Count - 1;
        storageElem.canStore.Q<Button>("Store").RegisterCallback<ClickEvent>((_) => ToggleCanStore(true, x));
        storageElem.canStore.Q<Button>("Not-Store").RegisterCallback<ClickEvent>((_) => ToggleCanStore(false, x));
        try
        {
            storageElem.icon.style.unityBackgroundImageTintColor = SceneRefs.infoWindow.resourceSkins.GetResourceColor(uiResource.type);
        }
        catch
        {
            Debug.LogError("Editor");
        }
        storageElem.label.text = uiResource.ammount.ToString();

        UpdateGroup(storageElem, x);

        storageElems.Add(storageElem);
        lastRow.Add(element);
    }

    void AddNewRow()
    {
        lastRow = new();
        lastRow.AddToClassList("storage-row");
        storageScroll.Add(lastRow);
    }
    #endregion

}
