using System;
using Unity.Properties;
using UnityEngine;

[Serializable]public class MoneyResource : ModifiableResource
{
    [SerializeField] ModifiableInteger money = new();
    [CreateProperty] public ModifiableInteger Money { get => money; set => money = value; }
}