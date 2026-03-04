using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class DataCategory<T> where T : DataObject
{
    [SerializeField] public int id;
    [SerializeField] public string Name;
    [SerializeReference] public List<T> Objects;
    [SerializeField] public VectorImage Icon;

    [NonSerialized] public List<T> availableObjects;
}