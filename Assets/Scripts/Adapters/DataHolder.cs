using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataObject
{
    [SerializeField] public string Name;
    [SerializeField] public int id;

    public virtual string GetName() => Name;

    public DataObject(int _id)
    {
        id = _id;
        Name = "";
    }

    public DataObject() { }
}

public abstract class DataHolder<CATEG_T, WRAPPER_T> : ScriptableObject where CATEG_T : DataCategory<WRAPPER_T> where WRAPPER_T : DataObject
{
    #region Editor
#if UNITY_EDITOR
    public List<string> Choices() => Categories.Select(q => q.Name).ToList();
    public int UniqueID()
    {
        int i;
        do
        {
            i = UnityEngine.Random.Range(0, int.MaxValue);
        } while (Categories.SelectMany(q => q.Objects).Count(q => q.id == i) > 0);
        return i;
    }
#endif
    #endregion

    [SerializeField] public List<CATEG_T> Categories;


}