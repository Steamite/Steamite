using System.Collections.Generic;
using UnityEngine;

public abstract class DataCategory<T>
{
    [SerializeField] public string Name;
    [SerializeField] public List<T> Objects;
    [SerializeField] public Texture2D Icon;
}