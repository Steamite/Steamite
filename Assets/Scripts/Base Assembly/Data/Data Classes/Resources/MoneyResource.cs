using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Properties;
using UnityEngine;

[Serializable]
public class MoneyResource : ModifiableResource
{
    [SerializeField] ModifiableInteger money = new();
    [CreateProperty] public ModifiableInteger Money { get => money; set => money = value; }

    public MoneyResource() : base() { }
    public MoneyResource(ResourceSave save, int _money) : base(save) 
    {
        money = new(_money);
    }
    public MoneyResource(Resource resAmmount) : base(resAmmount) 
    {
        if (resAmmount is MoneyResource mon)
            money = new(mon.money.currentValue);
    }
    public MoneyResource(List<ResourceType> types, List<int> ammounts) : base(types, ammounts) { }
    public MoneyResource(List<ResourceType> types, List<int> ammounts, int _money)
    {
        baseResource.types = types;
        baseResource.ammounts = ammounts;
        money = new(_money);//.BaseValue = _money;
        Init();
    }

    public static MoneyResource operator *(MoneyResource res, int multiplier)
    {
        MoneyResource moneyResource = new(res);
        for (int i = 0; i < moneyResource.types.Count; i++)
        {
            moneyResource.ammounts[i] *= multiplier;
        }
        moneyResource.money.currentValue *= multiplier;
        return moneyResource;
    }
    public override void Init()
    {
        ((IModifiable)Money).Init();
        base.Init();
    }
    public override string ToString()
    {
        string s = $"{money} Money";
        for (int i = 0; i < types.Count; i++)
        {
            s += $", {ammounts[i]} {types[i]}";
        }
        return s;
    }
}