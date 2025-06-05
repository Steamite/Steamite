using Mono.Cecil;
using System;
using UnityEngine;

[Serializable]
public class ModifiableResource : Resource
{
    [SerializeField] public Resource baseResource = new Resource();
#if UNITY_EDITOR
    public Resource EditorResource => baseResource;
#endif
    [NonSerialized] float modifier = 1;
    [NonSerialized] bool init = false;
    public float Modifier 
    { 
        get => modifier; 
        set 
        { 
            modifier = value;
            RecalculateCurrentResource();
        }
    }

    public ModifiableResource()
    {

    }

    void RecalculateCurrentResource()
    {
        if(init == false)
        {
            type = baseResource.type;
            ammount = baseResource.ammount;
        }

        for (int x = 0; x < baseResource.ammount.Count; x++)
        {
            ammount[x] = Mathf.RoundToInt(baseResource.ammount[x] * modifier);
        }
    }

    public void Init()
    {
        modifier = 1;
        init = false;
        RecalculateCurrentResource();
    }
    public void SetBaseRes(Resource _baseResouce)
    {
        baseResource = _baseResouce;
        modifier = 1;
        init = false;
        RecalculateCurrentResource();
    }
}