using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct AssigmentKey
{
    [SerializeField] public int id;
    [SerializeField] public Human h;
}
[Serializable]
public class DictionaryTest : IDictionary<AssigmentKey, Resource>
{
    public Resource this[AssigmentKey key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    [SerializeField] List<AssigmentKey> keys;
    public ICollection<AssigmentKey> Keys => keys;

    [SerializeField]List<Resource> values;
    public ICollection<Resource> Values => values;

    public int Count => throw new NotImplementedException();

    public bool IsReadOnly => throw new NotImplementedException();

    public void Add(AssigmentKey key, Resource value)
    {
        throw new NotImplementedException();
    }

    public bool ContainsKey(AssigmentKey key)
    {
        throw new NotImplementedException();
    }

    public bool Remove(AssigmentKey key)
    {
        throw new NotImplementedException();
    }

    public bool TryGetValue(AssigmentKey key, out Resource value)
    {
        throw new NotImplementedException();
    }

    public void Add(KeyValuePair<AssigmentKey, Resource> item)
    {
        throw new NotImplementedException();
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public bool Contains(KeyValuePair<AssigmentKey, Resource> item)
    {
        throw new NotImplementedException();
    }

    public void CopyTo(KeyValuePair<AssigmentKey, Resource>[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public bool Remove(KeyValuePair<AssigmentKey, Resource> item)
    {
        throw new NotImplementedException();
    }

    public IEnumerator<KeyValuePair<AssigmentKey, Resource>> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}