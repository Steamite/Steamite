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
    public void NewGameInit(ref Action humanActivation)
    {
        humen = new();
        int i = MyGrid.gridSize;
        GridPos pos = new(i / 2, 2, i / 2);
        for(i = 0; i < 3; i++)
        {
            // adding the human
            
            AddHuman(SceneRefs.objectFactory.CreateAHuman(pos, hatMaterial[i], i), ref humanActivation);
        }
    }

    public void AddHuman(Human h, ref Action action)
    {
        action += h.ActivateHuman;
        humen.Add(h);
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
