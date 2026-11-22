

using System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

public struct SelectionData
{
    public int localPosition;
    public Entity entityId;
}

public abstract class InfoWindowControl : VisualElement, IUIElement
{
    public virtual void Select(SelectionData data)
    {
        style.display = DisplayStyle.Flex;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>False if the update was invalid (data object doesn't exist anymore)</returns>
    public virtual bool Update()
    {
        throw new NotImplementedException();
    }


    public abstract void Open(object data);
    public InfoWindowControl()
    {
        style.flexGrow = 1;
    }
}

