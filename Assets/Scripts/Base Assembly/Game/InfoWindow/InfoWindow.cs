using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
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
    /// <summary><see cref="global::Vein"/> is selected.</summary>
    Vein,
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
    public IUIElement header;
    /// <summary>Info window itself.</summary>
    public VisualElement window;
    public VisualElement windowBody;
    public VisualElement secondWindow;
    public VisualElement secondBody;
    public VisualElement newWindow;

    TabView buildingTabView;

    public static bool CanZoom = true;

    #region Construction View
    Label constructionStateLabel;
    Button deconstructButton;
    #endregion

    /// <summary>List containing all active bindings, that are cleared on <see cref="Close(bool)"/>.</summary>
    //List<BindingContext> activeBindings;

    /// <summary>Stores last opened mode. To hide it and remove datasource.</summary>
    public InfoMode lastInfo { get; private set; }

    public Action<Building> buildingCostChange;

    InfoWindowControlHolder controls;
    #endregion

    public void CreateSecondWindow(string labelTitle)
    {
        (secondWindow[0][0] as Label).text = labelTitle;
        secondWindow.style.display = DisplayStyle.Flex;
        window.RegisterCallback<MouseEnterEvent>(MyOnMouseEnter);
        window.RegisterCallback<MouseLeaveEvent>(MyOnMouseExit);
    }
    public void CloseSecondWindow()
    {
        secondWindow.style.display = DisplayStyle.None;
        secondBody.Clear();
        window.UnregisterCallback<MouseEnterEvent>(MyOnMouseEnter);
        window.UnregisterCallback<MouseLeaveEvent>(MyOnMouseExit);
    }
    /// <summary>Fills all control references.</summary>
    public async Task BeforeInit()
    {
        lastInfo = InfoMode.None;
        VisualElement root = gameObject.GetComponent<UIDocument>().rootVisualElement;
        window = root.Q<VisualElement>("Info-Window");
        windowBody = window[1];
        secondWindow = root[1];
        secondBody = secondWindow[1];
        (secondWindow[0][1] as Button).clicked += CloseSecondWindow;
        controls = await Addressables.LoadAssetAsync<InfoWindowControlHolder>("Assets/Game Data/UI/InfoWindowControlHolder.asset").Task;

        window.style.display = DisplayStyle.None;

        VisualElement bar = window[0];
        header = bar[0] as IUIElement;
        bar.Q<Button>("Close").RegisterCallback<ClickEvent>((_) => SceneRefs.GridTiles.DeselectObjects());
    }

    #region Reseting Bindings
    /// <summary>
    /// Unbinds and hides the last opened view.
    /// </summary>
    /// <param name="hide">Defauly true, if true hide the window.</param>
    public void Close(bool hide = true)
    {
        window.UnregisterCallback<MouseEnterEvent>(MyOnMouseEnter);
        window.UnregisterCallback<MouseLeaveEvent>(MyOnMouseExit);
        CanZoom = true;

        if (hide)
            window.style.display = DisplayStyle.None;
        windowBody.Clear();

        secondWindow.style.display = DisplayStyle.None;
        secondBody.Clear();

        //activeBindings.Clear();
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
        buildingTabView = null;
        window.RegisterCallback<MouseEnterEvent>(MyOnMouseEnter);
        window.RegisterCallback<MouseLeaveEvent>(MyOnMouseExit);
        switch (active)
        {
            case InfoMode.None:
                break;
            case InfoMode.Water:
                controls.CreateElementByName("Water Info", windowBody, dataSource);
                break;
            case InfoMode.Vein:
                controls.CreateElementByName("Vein Info", windowBody, dataSource);
                break;

            case InfoMode.Building:
                Building building = (Building)dataSource;
                if (!building.constructed || building.deconstructing)
                {
                    controls.CreateElementByName("Construction Info", windowBody, dataSource);
                }
                else
                {
                    windowBody.Add(buildingTabView = new TabView()
                    {
                        name = "Building Info",
                        style =
                        {
                            flexGrow = 1
                        }
                    });
                }
                break;

            case InfoMode.Human:
                controls.CreateElementByName("Human Info", windowBody, dataSource);
                break;

            case InfoMode.Rock:
                controls.CreateElementByName("Rock Info", windowBody, dataSource);
                break;

            case InfoMode.Chunk:
                controls.CreateElementByName("Chunk Info", windowBody, dataSource);
                break;

            default:
                Close();
                break;
        }
    }

    private void MyOnMouseEnter(MouseEnterEvent evt)
    {
        CanZoom = false;
        Debug.Log("Cant " + evt.currentTarget);
    }

    private void MyOnMouseExit(MouseLeaveEvent evt)
    {
        CanZoom = true;
        Debug.Log("Can " + evt.currentTarget);
    }

    public void CreateBuildingControls(Dictionary<string, List<string>> controlsToCreate, Building building)
    {
        foreach (var key in controlsToCreate.Keys)
        {
            Tab activeTab;
            buildingTabView.Add(activeTab = new Tab(key)
            {
                style =
                {
                    flexGrow = 1
                }
            });
            VisualElement tabContentContainer = activeTab.hierarchy.Children().ElementAt(0);
            foreach (var control in controlsToCreate[key])
            {
                controls.CreateElementByName(control, tabContentContainer, building);
            }
        }
        if (controlsToCreate.Keys.Count == 1)
        {
            buildingTabView.hierarchy.Children().ElementAt(0).style.display = DisplayStyle.None;
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
        /*if (activeBindings.FindIndex(q => q.context == context.context && q.bindingId == context.bindingId) > -1)
            throw new NotSupportedException("This object already has a binding! Clear it first.");*/
        context.context.SetBinding(context.bindingId, binding);
        BindingResult res;
        context.context.TryGetLastBindingToUIResult(context.bindingId, out res);
        if (res.status == BindingStatus.Failure)
            Debug.Log(res.message);
        context.context.schedule.Execute(() =>
        {
            //activeBindings.Add(context);
            ((IUpdatable)dataObject).UIUpdate(binding.dataSourcePath.ToString());
        });
    }

    #endregion
}