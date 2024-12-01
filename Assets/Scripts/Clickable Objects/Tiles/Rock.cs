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
    public float integrity;

    // infuenced by the player
    public Human assigned;
    public bool toBeDug;
    // prefab to replace with
    public string assetPath;

    ///////////////////////////////////////////////////
    ///////////////////Overrides///////////////////////
    ///////////////////////////////////////////////////
    #region Basic Operations
    public override void UniqueID()
    {
        CreateNewId(transform.parent.GetComponentsInChildren<Rock>().Select(q => q.id).ToList());
    }
    public override void GetID(JobSave jobSave)
    {
        jobSave.objectId = id;
        jobSave.objectType = typeof(Rock);
    }
    #endregion Basic Operations
    #region Window
    public override InfoWindow OpenWindow(bool setUp = false)
    {
        InfoWindow info;
        // if selected
        if ((info = base.OpenWindow(setUp)) != null)
        {
            if (setUp)
            {
                info.SwitchMods(InfoMode.Rock, name); // set window mod to Ore Info
                info.clickObjectTransform.GetChild(2).GetChild(1).GetComponent<TMP_Text>().text = MyRes.GetDisplayText(rockYield);
            }
            info.clickObjectTransform.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = $"Integrity: {Math.Round(integrity,2)}";
            return info;
        }
        return null;
    }
    #endregion Window
    #region Saving
    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        if (clickable == null)
            clickable = new RockSave();
        if(rockYield.type.Count > 0)
        {
            (clickable as RockSave).res = rockYield.type[0];
            (clickable as RockSave).ammount = rockYield.ammount[0];
        }
        else
            (clickable as RockSave).ammount = -1;
        (clickable as RockSave).integrity = integrity;
        (clickable as RockSave).oreName = name;
        (clickable as RockSave).toBeDug = toBeDug;
        return base.Save(clickable);
    }
    public override void Load(ClickableObjectSave save)
    {
        integrity = (save as RockSave).integrity;
        toBeDug = (save as RockSave).toBeDug;
        name = (save as RockSave).oreName;
        if ((save as RockSave).ammount > -1)
        {
            rockYield.type[0] = (save as RockSave).res;
            rockYield.ammount[0] = (save as RockSave).ammount;
        }
        else
        {
            Color c = gameObject.GetComponent<MeshRenderer>().material.color;
            gameObject.GetComponent<MeshRenderer>().material.color = c / integrity * 2;
        }
        base.Load(save);
    }
    #endregion Saving
    public override GridPos GetPos()
    {
        return new GridPos(
            transform.position.x,
            (transform.position.y - 1.5f) / 2,
            transform.position.z);
    }

}