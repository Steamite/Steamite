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
    Resource resources;
    /// <summary>Money for upgrades and trading</summary>
    int money;
    /// <summary>Resource display on the top bar.</summary>
    IUIElement resourceList;
    /// <summary>Money bar in the top center.</summary>
    Label moneyLabel;
    #endregion

    #region Properties
    /// <inheritdoc cref="resources"/>
    [CreateProperty]
    public Resource GlobalResources
    {
        get => resources;
    }
    /// <inheritdoc cref="money"/>
    [CreateProperty]
    public int Money
    {
        get => money;
        set
        {
            money = value;
            UIUpdate(nameof(Money));
        }
    }
    #endregion

    #region Init
    /// <summary>
    /// Gets references and fills UI Elements.
    /// </summary>
    /// <param name="fillMoney">If the game is new then set default value for <see cref="Money"/>.</param>
    /// <returns>Empty Resources of all types.</returns>
    public Resource InitializeResources(bool fillMoney)
    {
        resources = new();
        if (fillMoney)
            Money = 2000;
        string[] names = Enum.GetNames(typeof(ResourceType));
        for (int i = 0; i < names.Length; i++)
        {
            resources.type.Add((ResourceType)i);
            resources.ammount.Add(0);
        }

        VisualElement root = gameObject.GetComponent<UIDocument>().rootVisualElement;

        moneyLabel = root.Q<Label>("Money-Value");
        DataBinding binding = BindingUtil.CreateBinding(nameof(Money));
        binding.sourceToUiConverters.AddConverter((ref int _Money) => $"{Money} <color=#FFD700>£</color>");
        moneyLabel.SetBinding("text", binding);
        moneyLabel.dataSource = this;


        resourceList = root.Q<ListView>("Resources") as IUIElement;
        resourceList.Open(this);
        return resources;
    }
    #endregion
}
