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
    [SerializeField] List<Human> humen;
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
            h.transform.localPosition = 
                new(h.transform.position.x, 2*2, h.transform.position.z);
            h.UniqueID();
            h.ActivateHuman();
        }
    }

    public void AddHuman(Human h, ref Action action)
    {
        action += h.ActivateHuman;
        humen.Add(h);
    }

    public void CreateHuman()
    {
        Human h = Instantiate(humanPref, new Vector3(10,1,10), Quaternion.identity, transform.GetChild(0));
        h.UniqueID();
        h.ActivateHuman();
    }

    public void SwitchLevel(int currentI, int newI)
    {
        humen.Where(q => q?.GetPos().y == currentI).ToList().
            ForEach(q => q.gameObject.SetActive(false));
        humen.Where(q => q?.GetPos().y == newI).ToList().
            ForEach(q => q.gameObject.SetActive(true));
    }
}
