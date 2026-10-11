using System;
using UnityEngine;
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

public interface IUIInfoWindow
{
    public void Close(bool hide = true);
    public void Open(object dataSource, InfoMode active);
}

/// <summary>Inspection window for everything in <see cref="InfoMode</summary>
public class InfoWindowAdapter : MonoBehaviour
{
    #region Variables

    // TODO connect maybe sometimes?
    public Action<Building> buildingCostChange;

    static InfoWindowAdapter instance;
    public static InfoWindowAdapter Window => instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    public static void Clear() => instance = null;

    IUIInfoWindow realInfoWindow;
    private void Awake()
    {
        realInfoWindow = GetComponent<IUIInfoWindow>();
        instance = this;
    }
    #endregion


    #region Opening
    public void Close(bool hide = true)
    {
        realInfoWindow.Close(hide);
    }

    /// <summary>
    /// Displays the Info window and attaches dataSource to the selected category.
    /// </summary>
    /// <param name="dataSource">Datasource to assign.</param>
    /// <param name="active">Where to assign the datasource.</param>
    /// <exception cref="NotImplementedException"><paramref name="active"/> was out of range.</exception>
    public void Open(object dataSource, InfoMode active)
    {
        realInfoWindow.Open(dataSource, active);
    }
    #endregion
}