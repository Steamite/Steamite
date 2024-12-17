using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Linq;
using Unity.Properties;

public enum InfoMode
{
    None,
    Building,
    Human,
    Rock,
    Chunk,
    Water,
    Research,
}

public struct BindingContext
{
    public VisualElement context;
    public BindingId bindingId;

    public BindingContext(VisualElement _context, string _bindingId)
    {
        context = _context;
        bindingId = new(_bindingId);
    }
}


public class InfoWindow : MonoBehaviour
{
    [SerializeField] VisualTreeAsset resourceItem;
    public ResourceSkins resourceSkins;

    // universal shorcuts
    public Label header;
    public VisualElement window;

    // main shorcuts
    public VisualElement buildingElement;
    public VisualElement humanElement;
    VisualElement rockChunkElement;
    public VisualElement researchElement;

    // additional shorcuts
    public VisualElement inConstructionElement;
    public VisualElement constructedElement;

    //Unlocked resources
    /// <summary>
    /// List containing all active bindings, that are cleared on <see cref="Close"/>.
    /// </summary>
    List<BindingContext> activeBindings;

    InfoMode lastInfo;


    void Awake()
    {
        activeBindings = new();
        lastInfo = InfoMode.None;
        window = gameObject.GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("Info-Window");
        window.style.display = DisplayStyle.None;

        header = window.Q<Label>("Header");
        header.parent.Q<Button>("Close").RegisterCallback<ClickEvent>((_) => SceneRefs.gridTiles.DeselectObjects());

        buildingElement = window.Q<VisualElement>("Building");
        buildingElement.style.display = DisplayStyle.None;
        humanElement = window.Q<VisualElement>("Human");
        humanElement.style.display = DisplayStyle.None;
        rockChunkElement = window.Q<VisualElement>("Rock-Chunk");
        rockChunkElement.style.display = DisplayStyle.None;

        inConstructionElement = buildingElement.Q<VisualElement>("Construction-View");
        constructedElement = buildingElement.Q<VisualElement>("Constructed");

        //constructed.Q<WorkerAssign>("Worker-Assign").Init();
       // (()constructedElement.Q<VisualElement>("Production")).Init();
        
    }

    #region Reseting Bindings
    /// <summary>
    /// Unbinds and hides the last opened view.
    /// </summary>
    /// <param name="hide">Defauly true, if true hide the window.</param>
    public void Close(bool hide = true)
    {
        if (hide)
            window.style.display = DisplayStyle.None;

        foreach (BindingContext context in activeBindings)
            context.context.ClearBinding(context.bindingId);
        activeBindings = new();

        switch (lastInfo)
        {
            case InfoMode.None:
            case InfoMode.Water:
            case InfoMode.Research:
                break;
            case InfoMode.Building:
                ResetView(buildingElement);
                break;
            case InfoMode.Human:
                ResetView(humanElement);
                break;
            case InfoMode.Rock:
                ResetView(rockChunkElement);
                break;
        }
    }
    void ResetView(VisualElement element)
    {
        element.style.display = DisplayStyle.None;
        element.dataSource = null;
        element.dataSourceType = null;
    }
    #endregion

    #region Opening
    public void Open(object dataSource, InfoMode active)
    {
        if (active != lastInfo)
        {
            Close(false);
            lastInfo = active;
        }
        window.style.display = DisplayStyle.Flex;
        DataBinding binding;

        switch (active)
        {
            case InfoMode.None:
            case InfoMode.Water:
            case InfoMode.Research:
                throw new NotImplementedException();
            case InfoMode.Building:
                buildingElement.dataSource = dataSource;
                buildingElement.style.display = DisplayStyle.Flex;
                break;

            case InfoMode.Human:
                humanElement.dataSource = dataSource;
                ((IUIElement)humanElement.Q<ListView>("Inventory")).Fill(dataSource);
                humanElement.Q<Label>("Specialization-Value").text = ((Human)dataSource).specialization.ToString();

                // Efficiency Binding
                binding = CreateBinding(nameof(Human.Efficiency));
                binding.sourceToUiConverters.AddConverter((ref Efficiency efficiency) => $"{efficiency.efficiency:0.#}");
                AddBinding(new(humanElement.Q<Label>("Efficiency-Value"), "text"), binding, dataSource);

                // Job Binding
                binding = CreateBinding(nameof(Human.Job));
                binding.sourceToUiConverters.AddConverter((ref JobData jobData) => $"{jobData.job}");
                AddBinding(new(humanElement.Q<Label>("Type-Value"), "text"), binding, dataSource);

                // Pos Binding
                binding = CreateBinding(nameof(Human.Job));
                binding.sourceToUiConverters.AddConverter((ref JobData jobData) => $"{(jobData.interest ? jobData.interest.GetPos() : "None")}");
                AddBinding(new(humanElement.Q<Label>("Position-Value"), "text"), binding, dataSource);

                // Object Binding
                binding = CreateBinding(nameof(Human.Job));
                binding.sourceToUiConverters.AddConverter((ref JobData jobData) => $"{(jobData.interest ? jobData.interest.name : "None")}");
                AddBinding(new(humanElement.Q<Label>("Interest-Value"), "text"), binding, dataSource);

                humanElement.style.display = DisplayStyle.Flex;
                break;

            case InfoMode.Rock:
                rockChunkElement.dataSource = dataSource;
                ((IUIElement)rockChunkElement.Q<ListView>("Yield")).Fill(dataSource);

                // Assigned Binding
                binding = CreateBinding(nameof(Rock.Assigned));
                binding.sourceToUiConverters.AddConverter((ref Human human) => $"{(human ? human.name: "None")}");
                AddBinding(new(rockChunkElement.Q<Label>("Assigned-Value"), "text"), binding, dataSource);

                // Integrity Binding
                binding = CreateBinding(nameof(Rock.Integrity));
                binding.sourceToUiConverters.AddConverter((ref float integrity) => $"{integrity:0.#}");
                AddBinding(new(rockChunkElement.Q<Label>("Integrity-Value"), "text"), binding, dataSource);

                rockChunkElement.style.display = DisplayStyle.Flex;
                ToggleChildElems(rockChunkElement.Q<VisualElement>("Text"), new() { "Assign", "Integrity" });
                break;

            case InfoMode.Chunk:
                rockChunkElement.dataSource = dataSource;
                ((IUIElement)rockChunkElement.Q<ListView>("Yield")).Fill(dataSource);

                // Assigned Binding
                binding = CreateBinding(nameof(Chunk.LocalRes));
                binding.sourceToUiConverters.AddConverter((ref StorageResource res) => $"{(res.carriers.Count > 0 ? res.carriers.First().name : "None")}");
                AddBinding(new(rockChunkElement.Q<Label>("Assigned-Value"), "text"), binding, dataSource);

                rockChunkElement.style.display = DisplayStyle.Flex;
                ToggleChildElems(rockChunkElement.Q<VisualElement>("Text"), new() {"Assign"});
                break;
        }
    }

    public void ToggleChildElems(VisualElement element, List<string> toEnable) =>
        element.Children().ToList().ForEach(
            q => q.style.display = toEnable.Contains(q.name)
            ? DisplayStyle.Flex : DisplayStyle.None);

    #endregion

    #region Binding Creation
    public DataBinding CreateBinding(string varName)
    {
        return new DataBinding
        {
            dataSourcePath = new PropertyPath(varName),
            bindingMode = BindingMode.ToTarget
        };
    }

    public void AddBinding(BindingContext context, DataBinding binding, object dataSource)
    {
        context.context.SetBinding(context.bindingId, binding);
        activeBindings.Add(context);
        ((IUpdatable)dataSource).UpdateWindow(binding.dataSourcePath.ToString());
    }
    #endregion
}