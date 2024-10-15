using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;
public class Humans : MonoBehaviour
{
    public InfoWindow infoWindow;
    public GridTiles grid;
    public LoadingScreen loadingScreen;
    public List<Human> humen;
    public Tick ticks;
    public EfficencyModifiers modifiers;

    [SerializeField] Human humanPref;
    //Don't missmatch with "Human" this is a script for the parent object, no inheritence thou
    public void GetHumans()
    {
        humen = new();
        humen = transform.GetComponentsInChildren<Human>().ToList();
        foreach(Human h in humen)
        {
            h.UniqueID();
            h.ActivateHuman();
        }
    }
    public void AddHuman(Human h, ref Action action)
    {
        action += h.ActivateHuman;
        humen.Add(h);
    }

    internal void CreateHuman()
    {
        Human h = Instantiate(humanPref, new Vector3(10,1,10), Quaternion.identity, transform.GetChild(0));
        h.UniqueID();
        h.ActivateHuman();
    }
}
