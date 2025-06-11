using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>All groups of objects that can be inspected. For switching info window views.</summary>
public enum InfoMode
{
    /// <summary>Nothing is selected.</summary>
    None,
    /// <summary><see cref="global::Building"/> is selected.</summary>
    Building,
    /// <summary><see cref="global::Human"/> is selected.</summary>
    Human,
    /// <summary><see cref="global::Rock"/> is selected.</summary>
    Rock,
    /// <summary><see cref="global::Chunk"/> is selected.</summary>
    Chunk,
    /// <summary><see cref="global::Water"/> is selected.</summary>
    Water,
}

/// <summary>
/// A temporary binding context that only exists for the time of inspection.<br/>
/// Used by ALL <see cref="InfoWindow"/> bindings.
/// </summary>
public class BindingContext
{
    public VisualElement context;
    public BindingId bindingId;
    public bool clearDataSource;

    public BindingContext(VisualElement _context, string _bindingId, bool _clearDataSource = false)
    {
        context = _context;
        bindingId = new(_bindingId);
        clearDataSource = _clearDataSource;
    }

    public void ClearBinding()
    {
        context.ClearBinding(bindingId);
        if (clearDataSource)
            context.dataSource = null;
    }
}


/// <summary>Inspection window for everithing in <see cref="InfoMode</summary>
public class InfoWindow : MonoBehaviour, IBeforeLoad
{
    #region Variables
    /// <summary>For styling resouces in UI elements.</summary>
    //public ResourceSkins resourceSkins;

    /// <summary>Info window text header.</summary>
    public Label header;
    /// <summary>Info window itself.</summary>
    public VisualElement window;

    /// <summary><see cref="Building"/> group.</summary>
    VisualElement buildingElement;
    /// <summary><see cref="Human"/> group.</summary>
    VisualElement humanElement;
    /// <summary><see cref="Rock"/> && <see cref="Chunk"/> group.</summary>
    VisualElement rockChunkElement;
    /// <summary><see cref="ResearchNode"/> group.</summary>
    VisualElement researchElement;

    /// <summary>View for buildings that are beeing constructed.</summary>
    public VisualElement inConstructionElement;
    #region Construction View
    Label constructionStateLabel;
    Button deconstructButton;
    #endregion
    /// <summary>View for buildings that are constructed.</summary>
    public VisualElement constructedElement;

    /// <summary>List containing all active bindings, that are cleared on <see cref="Close(bool)"/>.</summary>
    List<BindingContext> activeBindings;

    /// <summary>Stores last opened mode. To hide it and remove datasource.</summary>
    public InfoMode lastInfo { get; private set; }
    #endregion

    /// <summary>Fills all control references.</summary>
    public IEnumerator Init()
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
        constructionStateLabel = buildingElement.Q<Label>("State-Label");
        deconstructButton = buildingElement.Q<Button>("State-Change");
        deconstructButton.clicked += 
            () => 
            {
                Building b = (Building)buildingElement.dataSource;
                b.OrderDeconstruct();
                if(b != null)
                    UpdateConstructionText(b);
            };

        constructedElement = buildingElement.Q<VisualElement>("Constructed");
        yield return null;
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

        ClearBindings();

        switch (lastInfo)
        {
            case InfoMode.None:
            case InfoMode.Water:
                break;
            case InfoMode.Building:
                ResetView(buildingElement);
                break;
            case InfoMode.Human:
                ResetView(humanElement);
                break;
            case InfoMode.Rock:
            case InfoMode.Chunk:
                ResetView(rockChunkElement);
                break;
        }
    }
    /// <summary>Clears all active bindings.</summary>
    void ClearBindings()
    {
        foreach (BindingContext context in activeBindings)
            context.ClearBinding();
        activeBindings = new();
    }

    /// <summary>
    /// Clears all bindings on a single Visual Element.
    /// </summary>
    /// <param name="element">Element to clear</param>
    public void ClearBinding(VisualElement element)
    {
        BindingContext context = activeBindings.FirstOrDefault(q => q.context == element);
        if (context != null)
        {
            activeBindings.Remove(context);
            context.ClearBinding();
        }
    }

    /// <summary>
    /// Hides and clears <paramref name="element"/>.
    /// </summary>
    /// <param name="element">Element to hide.</param>
    void ResetView(VisualElement element)
    {
        element.style.display = DisplayStyle.None;
        element.dataSource = null;
        element.dataSourceType = null;
    }
    #endregion

    #region Opening
    /// <summary>
    /// Displays the Info window and attaches dataSource to the selected category.
    /// </summary>
    /// <param name="dataSource">Datasource to assign.</param>
    /// <param name="active">Where to assign the datasource.</param>
    /// <exception cref="NotImplementedException"><paramref name="active"/> was out of range.</exception>
    public void Open(object dataSource, InfoMode active)
    {
        Close(false);
        lastInfo = active;
        window.style.display = DisplayStyle.Flex;
        DataBinding binding;

        switch (active)
        {
            case InfoMode.None:
            case InfoMode.Water:
                throw new NotImplementedException();

            case InfoMode.Building:
                // DEBUG_Binding start of binding
                // shows the element and assignes it a datasource(the selected object), which is used for all child elements.
                buildingElement.style.display = DisplayStyle.Flex;
                buildingElement.dataSource = dataSource;
                buildingElement.dataSourceType = dataSource.GetType();

                Building building = (Building)dataSource;
                if (!building.constructed || building.deconstructing)
                {
                    ToggleChildElems(buildingElement, new() { "Construction-View" });
                    binding = BindingUtil.CreateBinding(nameof(building.constructionProgress));
                    binding.sourceToUiConverters.AddConverter((ref float progress) => $"Progress: {(progress / building.maximalProgress) * 100:0}%");
                    RegisterTempBinding(new(inConstructionElement.Q<Label>("Progress"), "text"), binding, building);
                     
                    if (building.deconstructing)
                    {
                        buildingElement.Q<VisualElement>("Resources").style.display = DisplayStyle.None;
                        UpdateConstructionText(building);
                    }
                    else
                    {
                        buildingElement.Q<VisualElement>("Resources").style.display = DisplayStyle.Flex;
                        UpdateConstructionText(building);
                        ((IUIElement)buildingElement.Q<VisualElement>("Resources")).Open(new Tuple<Building, Action<bool>>(building,
                                (resDone) =>
                                {
                                    if (!resDone)
                                        constructionStateLabel.text = "waiting for resources";
                                    else if (building.deconstructing)
                                        constructionStateLabel.text = "deconstructing";
                                    else
                                        constructionStateLabel.text = "constructing";
                                }));
                    }
                }
                else 
                    ToggleChildElems(buildingElement, new() { "Constructed" });

                break;

            case InfoMode.Human:
                humanElement.dataSource = dataSource;
                ((IUIElement)humanElement.Q<ListView>("Inventory")).Open(dataSource);
                humanElement.Q<Label>("Specialization-Value").text = ((Human)dataSource).specialization.ToString();

                // Efficiency Binding
                binding = BindingUtil.CreateBinding(nameof(Human.Efficiency));
                binding.sourceToUiConverters.AddConverter((ref Efficiency efficiency) => $"{efficiency.efficiency:0.#}");
                RegisterTempBinding(new(humanElement.Q<Label>("Efficiency-Value"), "text"), binding, dataSource);

                // Job Binding
                binding = BindingUtil.CreateBinding(nameof(Human.Job));
                binding.sourceToUiConverters.AddConverter((ref JobData jobData) => $"{jobData.job}");
                RegisterTempBinding(new(humanElement.Q<Label>("Type-Value"), "text"), binding, dataSource);

                // Pos Binding
                binding = BindingUtil.CreateBinding(nameof(Human.Job));
                binding.sourceToUiConverters.AddConverter((ref JobData jobData) => $"{(jobData.interest ? jobData.interest.GetPos() : "None")}");
                RegisterTempBinding(new(humanElement.Q<Label>("Position-Value"), "text"), binding, dataSource);

                // Object Binding
                binding = BindingUtil.CreateBinding(nameof(Human.Job));
                binding.sourceToUiConverters.AddConverter((ref JobData jobData) => $"{(jobData.interest ? jobData.interest.objectName : "None")}");
                RegisterTempBinding(new(humanElement.Q<Label>("Interest-Value"), "text"), binding, dataSource);

                humanElement.style.display = DisplayStyle.Flex;
                break;

            case InfoMode.Rock:
                rockChunkElement.dataSource = dataSource;
                ((IUIElement)rockChunkElement.Q<ListView>("Yield")).Open(dataSource);

                // Assigned Binding
                binding = BindingUtil.CreateBinding(nameof(Rock.Assigned));
                binding.sourceToUiConverters.AddConverter((ref Human human) => $"{(human ? human.objectName : "None")}");
                RegisterTempBinding(new(rockChunkElement.Q<Label>("Assigned-Value"), "text"), binding, dataSource);

                // Integrity Binding
                binding = BindingUtil.CreateBinding(nameof(Rock.Integrity));
                binding.sourceToUiConverters.AddConverter((ref float integrity) => $"{integrity:0.#}");
                RegisterTempBinding(new(rockChunkElement.Q<Label>("Integrity-Value"), "text"), binding, dataSource);

                rockChunkElement.style.display = DisplayStyle.Flex;
                ToggleChildElems(rockChunkElement.Q<VisualElement>("Text"), new() { "Assign", "Integrity" });
                break;

            case InfoMode.Chunk:
                rockChunkElement.dataSource = dataSource;
                ((IUIElement)rockChunkElement.Q<ListView>("Yield")).Open(dataSource);

                // Assigned Binding
                binding = BindingUtil.CreateBinding(nameof(Chunk.LocalRes));
                binding.sourceToUiConverters.AddConverter((ref StorageResource res) => $"{(res.carriers.Count > 0 ? res.carriers.First().objectName : "None")}");
                RegisterTempBinding(new(rockChunkElement.Q<Label>("Assigned-Value"), "text"), binding, dataSource);

                rockChunkElement.style.display = DisplayStyle.Flex;
                ToggleChildElems(rockChunkElement.Q<VisualElement>("Text"), new() { "Assign" });
                break;
        }
    }

    void UpdateConstructionText(Building building)
    {
        if (building.constructed)
        {
            if (building.deconstructing)
            {
                constructionStateLabel.text = "Deconstructing";
                deconstructButton.text = "Cancel deconstruction";
            }
            else
            {
                constructionStateLabel.text = "Reconstructing";
                deconstructButton.text = "Deconstruct";
            }
        }
        else
        {
            if (building.deconstructing)
            {
                constructionStateLabel.text = "Removing construction";
                deconstructButton.text = "Continue construction";
            }
            else
            {
                if (MyRes.DiffRes(building.cost, building.LocalRes).Sum() > 0)
                    constructionStateLabel.text = "Waiting for resources";
                else
                    constructionStateLabel.text = "Constructing";
                deconstructButton.text = "Cancel construction";
            }
        }
    }

    /// <summary>
    /// Enables selected elements and disables others.
    /// </summary>
    /// <param name="element">Parent element.</param>
    /// <param name="toEnable">List of child elements to enable.</param>
    public void ToggleChildElems(VisualElement element, List<string> toEnable)
    {
        element.Children().ToList().ForEach(
            q => q.style.display = toEnable.Contains(q.name)
            ? DisplayStyle.Flex : DisplayStyle.None);
    }

    /// <summary>
    /// Also fills them selected elements and disables others.
    /// </summary>
    /// <param name="element">Parent element.</param>
    /// <param name="toEnable">List of child elements to enable.</param>
    public void ToggleChildElems(VisualElement element, List<string> toEnable, Building building)
    {
        // DEBUG_Binding - toggling elements
        // takes which elements are needed (toEnable) and shows + inits them.
        element.Children().ToList().ForEach(
            q => q.style.display = toEnable.Contains(q.name)
            ? DisplayStyle.Flex : DisplayStyle.None);
        foreach (string s in toEnable)
        {
            VisualElement elem = element.Q<VisualElement>(s);
            if (elem is IUIElement)
                ((IUIElement)elem).Open(building);
        }
    }

    #endregion

    #region Binding Creation
    /// <summary>
    /// Adds temporary binding, doesn't attach a datasource, because Info window has already attached it to the needed module.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <param name="dataObject"></param>
    /// <exception cref="NotSupportedException">Forgeting to unregister bindings.</exception>
    public void RegisterTempBinding(BindingContext context, DataBinding binding, object dataObject)
    {
        // DEBUG_Binding Binding register
        // Creates the binding with passed paremeters.
        if (activeBindings.FindIndex(q => q.context == context.context && q.bindingId == context.bindingId) > -1)
            throw new NotSupportedException("This object already has a binding! Clear it first.");
        context.context.SetBinding(context.bindingId, binding);
        context.context.schedule.Execute(() =>
        {
            activeBindings.Add(context);

            // Uses the binding to update UI (look inside ClickableObject(163) for implementation)
            // if changeEvent is null print a warning
            ((IUpdatable)dataObject).UIUpdate(binding.dataSourcePath.ToString());
        });
    }

    #endregion
}