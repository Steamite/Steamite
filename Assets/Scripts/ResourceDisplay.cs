using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

public class ResourceDisplay : MonoBehaviour, IUpdatable
{
    public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;
    public void UIUpdate(string property = "")
    {
        propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
    }

    Resource resources;
    [CreateProperty] 
    public Resource GlobalResources 
    {
        get => resources;
    }

    int money;
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

    IUIElement resourceList;
    Label moneyLabel;

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
        DataBinding binding = Util.CreateBinding(nameof(Money));
        binding.sourceToUiConverters.AddConverter((ref int _Money) => $"{Money} <color=#FFD700>£</color>");
        moneyLabel.SetBinding("text", binding);
        moneyLabel.dataSource = this;


        resourceList = root.Q<ListView>("Resources") as IUIElement;
        resourceList.Fill(this);
        return resources;
    }
}
