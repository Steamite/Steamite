using System;
using System.Threading.Tasks;
using UnityEngine;

public abstract class IGameDataController<T> : MonoBehaviour
{
    public Action OnLoad;

    public abstract Task LoadState(T saveData);

    protected void AfterLoad()
    {
        OnLoad?.Invoke();
    }

    public abstract T SaveState();
}