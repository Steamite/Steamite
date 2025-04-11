using System.Collections.Generic;
using UnityEngine;

public abstract class DataHolder<T> : ScriptableObject
{
	public abstract List<string> Choices();
	[SerializeField] public List<T> Categories;
}