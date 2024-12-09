using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class Rock : ClickableObject
{
    /// <summary>
    /// data about the rock(set)
    /// </summary>
    public Resource rockYield;
    /// <summary>
    /// public int hardness;
    /// </summary>
    public float integrity;

    /// <summary>
    /// infuenced by the player
    /// </summary>
    public Human assigned;
    public bool toBeDug;

    /// <summary>
    /// prefab to replace with
    /// </summary>
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
    protected override void SetupWindow(InfoWindow info)
    {
        base.SetupWindow(info); 
        info.SwitchMods(InfoMode.Rock); // set window mod to Ore Info
        info.FillResourceList(info.rock.Q<VisualElement>("Yield"), rockYield);
    }

    protected override void UpdateWindow(InfoWindow info)
    {
        base.UpdateWindow(info);
        info.rock.Q<Label>("Integrity-Value").text = $"{integrity:0.#}";
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
            ColorWithIntegrity();
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

    public void ColorWithIntegrity()
    {
        float f = 1;
        Color c = gameObject.GetComponent<MeshRenderer>().material.color;
        switch (integrity)
        {
            case < 2:
                f = 1f;
                break;
            case < 4:
                f = 0.8f;
                break;
            case < 6:
                f = 0.6f;
                break;
            case <11:
                f = 0.4f;
                break;
            default:
                f = 0.2f;
                break;
        }
        gameObject.GetComponent<MeshRenderer>().material.color = c * f;
    }
}