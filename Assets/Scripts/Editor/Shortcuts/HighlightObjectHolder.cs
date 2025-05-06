using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Highlight holder", menuName = "Editor/Highlight Holder", order = 1)]
public class HighlightObjectHolder : ScriptableObject
{

    [SerializeField] public List<HighlightObject> highlightObjects;

    public int FindObject(string _name)
    {
        for (int i = 0; i < highlightObjects.Count; i++)
        {
            foreach (string __name in highlightObjects[i].objects)
            {
                if (__name == _name)
                    return i;
            }
        }
        return -1;
    }
}
