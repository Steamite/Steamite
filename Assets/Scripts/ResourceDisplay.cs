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
    public bool HasActiveBinding() => propertyChanged != null;
    #endregion

    #region Variables
    /// <summary>Unused stored resources.</summary>
    MoneyResource resources = new();

    /// <summary>Resource display on the top bar.</summary>
    IUIElement resourceList;
    /// <summary>Money bar in the top center.</summary>
    Label moneyLabel;
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
    public Resource InitializeResources()
    {
        string[] names = Enum.GetNames(typeof(ResourceType));
        for (int i = 1; i < names.Length; i++)
        {
            resources.type.Add((ResourceType)i);
            resources.ammount.Add(0);
        }

        VisualElement root = gameObject.GetComponent<UIDocument>().rootVisualElement;

        moneyLabel = root.Q<Label>("Money-Value");
        DataBinding binding = BindingUtil.CreateBinding(nameof(Money));
        binding.sourceToUiConverters.AddConverter((ref int _Money) => $"{Money} <color=#FFD700>" + (char)163 + "</color>");
        moneyLabel.SetBinding("text", binding);
        moneyLabel.dataSource = this;

        resourceList = root.Q<ListView>("Resources") as IUIElement;
        resourceList.Open(this);
        return resources;
    }
    #endregion
}
