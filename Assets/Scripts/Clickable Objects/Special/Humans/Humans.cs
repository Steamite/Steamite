using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;
public class Humans : MonoBehaviour
{
    public InfoWindow infoWindow;
    public GridTiles grid;
    [SerializeField] List<Human> humen;
    public Tick ticks;
    public EfficencyModifiers modifiers;

    [SerializeField] Human humanPref;
    [SerializeField] List<Material> hatMaterial;
    //Don't missmatch with "Human" this is a script for the parent object, no inheritence thou
    public void NewGameInit()
    {
        humen = new();
        for(int i = 0; i < 3; i++)
        {
            CreateHuman().transform.GetChild(1).GetComponent<MeshRenderer>().material = hatMaterial[i];
            humen[^1].gameObject.name = $"Human {(i == 0 ? "Red" : i == 1 ? "Yellow" : "White")}";
        }
    }

    public void AddHuman(Human h, ref Action action)
    {
        action += h.ActivateHuman;
        humen.Add(h);
    }

    public Human CreateHuman()
    {
        Human h = Instantiate(humanPref, new Vector3(10,4,8), Quaternion.identity, transform.GetChild(0));
        h.UniqueID();
        humen.Add(h);
        h.ActivateHuman();
        return h;
    }

    public void SwitchLevel(int currentI, int newI)
    {
        humen.Where(q => q?.GetPos().y == currentI).ToList().
            ForEach(q => q.gameObject.SetActive(false));
        humen.Where(q => q?.GetPos().y == newI).ToList().
            ForEach(q => q.gameObject.SetActive(true));
    }

    public HumanSave[] SaveHumans()
    {
        return humen.Select(q => q.Save() as HumanSave).ToArray();
    }

    public void LoadHumans(IProgress<int> progress, HumanSave[] humanSaves, ref Action humanActivation)
    {
        humen = new();
        foreach (HumanSave hSave in humanSaves)
        {            
            AddHuman(SceneRefs.objectFactory.CreateSavedHuman(hSave), ref humanActivation);
            //progress.Report(progressGlobal++);
        }
    }
}
