using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class DataCategory<T>
{
#if UNITY_EDITOR_WIN
	public abstract int UniqueID();
#endif
	[SerializeField] public string Name;
	[SerializeField] public List<T> Objects;
	[SerializeField] public Texture2D Icon;
}