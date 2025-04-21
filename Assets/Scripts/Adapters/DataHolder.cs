using System.Collections.Generic;
using UnityEngine;

public abstract class DataHolder<T> : ScriptableObject
{
#if UNITY_EDITOR
    public abstract List<string> Choices();
#endif

    [SerializeField] public List<T> Categories;
}