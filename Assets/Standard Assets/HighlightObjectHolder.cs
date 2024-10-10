using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Highlight holder", menuName = "ScriptableObjects/Highlight Holder", order = 1)]
public class HighlightObjectHolder : ScriptableObject
{

    [SerializeField] public List<HighlightObject> highlightObjects = new();

    public int FindObject(string _name)
    {
        for (int i = 0; i < highlightObjects.Count; i++)
        {
            foreach(string __name in highlightObjects[i].objects)
            {
                if (__name == _name)
                    return i;
            }
        }
        return -1;
    }
}
