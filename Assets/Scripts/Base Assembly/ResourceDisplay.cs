using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Class for displaying global resources and money.
/// </summary>
public class ResourceDisplay : MonoBehaviour, IUpdatable
{
    #region UI UPDATE
    public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;
    public void UIUpdate(string property = "")
    {
        propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
    }
    #endregion

    #region Variables
    /// <summary>Unused stored resources.</summary>
    MoneyResource resources = new();

    #endregion

    #region Properties
    /// <inheritdoc cref="resources"/>
    [CreateProperty]
    public MoneyResource GlobalResources
    {
        get => resources;
    }
    /// <inheritdoc cref="money"/>
    [CreateProperty]
    public int Money
    {
        get => +resources.Money;
        set
        {
            resources.Money.currentValue = value;
            UIUpdate(nameof(Money));
            UIUpdate(nameof(GlobalResources));
        }
    }
    #endregion

    #region Init
    /// <summary>
    /// Gets references and fills UI Elements.
    /// </summary>
    /// <param name="fillMoney">If the game is new then set default value for <see cref="Money"/>.</param>
    /// <returns>Empty Resources of all types.</returns>
    public void InitializeResources()
    {
        resources.types = ResFluidTypes.GetResList();
        for (int i = 0; i < resources.types.Count; i++)
        {
            resources.ammounts.Add(0);
        }
    }
    #endregion
}
