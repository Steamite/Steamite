using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class DataCategory<T> where T : DataObject
{
    [SerializeField] public string Name;
    [SerializeField] public List<T> Objects;
    [SerializeField] public Texture2D Icon;

    [NonSerialized] public List<T> availableObjects;
}