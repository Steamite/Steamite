using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TMPro;
using UnityEngine;

public class Rock : ClickableObject
{
    // data about the rock(set)
    public Resource rockYield;
   // public int hardness;
    public int integrity;

    // infuenced by the player
    public Human assigned;
    public bool toBeDug;
    // prefab to replace with
    public string assetPath;

    public override void UniqueID()
    {
        CreateNewId(transform.parent.GetComponentsInChildren<Rock>().Select(q => q.id).ToList());
    }
    public override void GetID(JobSave jobSave)
    {
        jobSave.objectId = id;
        jobSave.objectType = typeof(Rock);
    }

    public override InfoWindow OpenWindow(bool setUp = false)
    {
        InfoWindow info;
        // if selected
        if ((info = base.OpenWindow(setUp)) != null)
        {
            if (setUp)
            {
                info.SwitchMods(2, name); // set window mod to Ore Info
                info.transform.GetChild(1).GetChild(2).GetChild(1).GetComponent<TMP_Text>().text = MyRes.GetDisplayText(rockYield);
            }
            info.transform.GetChild(1).GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = $"Integrity: {integrity}";
            return info;
        }
        return null;
    }

    public void ChunkCreation(Chunk chunk)
    {
        GridPos gridPos = new(transform.position);
        chunk = Instantiate(chunk, new Vector3(gridPos.x, gridPos.level + 0.75f, gridPos.z), chunk.transform.rotation, GameObject.FindWithTag("Chunks").transform); // spawns chunk of resources
        chunk.transform.GetChild(1).GetComponent<MeshRenderer>().material = GetComponent<MeshRenderer>().material;
        chunk.transform.GetChild(1).GetComponent<MeshRenderer>().material.DisableKeyword("_EMISSION");
        chunk.GetComponent<Chunk>().Create(rockYield, true);
    }

    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        if (clickable == null)
            clickable = new RockSave();
        (clickable as RockSave).integrity = integrity;
        (clickable as RockSave).oreName = name;
        (clickable as RockSave).toBeDug = toBeDug;
        return base.Save(clickable);
    }

    public override void Load(ClickableObjectSave save)
    {
        integrity = (save as RockSave).integrity;
        toBeDug = (save as RockSave).toBeDug;
        name = name.Replace("(Clone)", "");
        base.Load(save);
    }
}