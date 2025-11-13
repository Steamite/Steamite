using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine.UIElements;

public class SaveResourceCell : ResourceCell
{
    ResourceSave save;
    public SaveResourceCell() : base()
    {
        showAddRemoveFooter = true;
    }
    protected override void ChangeType(ChangeEvent<string> evt)
    {
        int i = evt.target.GetRowIndex(false);
        DataAssign assign = ResFluidTypes.GetSaveIndex(ResFluidTypes.GetTypeByName(evt.newValue));
        int j = save.types.IndexOf(assign);
        if (j != -1)
        {
            save.ammounts[j] += save.ammounts[i];
            save.types.RemoveAt(i);
            save.ammounts.RemoveAt(i);
            itemsSource = ToUIRes(new(save));
        }
        else
            save.types[i] = assign;
        EditorUtility.SetDirty(whatToSave);
    }

    /// <summary>
    /// Changes the ammount of a given type (<paramref name="evt"/>).
    /// </summary>
    /// <param name="evt">Event with the new value and changed element.</param>
    protected override void ChangeVal(ChangeEvent<int> evt)
    {
        int i = evt.target.GetRowIndex(false);
        save.ammounts[i] = evt.newValue;
        EditorUtility.SetDirty(whatToSave);
    }


    public void Open(ResourceSave _save, UnityEngine.Object _whatToSave)
    {
        save = _save;
        whatToSave = _whatToSave;
        resources = ToUIRes(new(save));
    }

    protected override void Add(BaseListView _)
    {
        save.types.Add(ResFluidTypes.GetSaveIndex(ResFluidTypes.None));
        save.ammounts.Add(0);
        itemsSource = ToUIRes(new(save));
        EditorUtility.SetDirty(whatToSave);
    }

    protected override void Remove(BaseListView el)
    {
        if (el.selectedIndex > -1 && el.selectedIndex < itemsSource.Count)
        {
            if (selectedIndex == itemsSource.Count - 1)
                allowRemove = false;
            save.types.RemoveAt(el.selectedIndex);
            save.ammounts.RemoveAt(el.selectedIndex);
            itemsSource = ToUIRes(new(save));
            EditorUtility.SetDirty(whatToSave);
        }
    }
}
