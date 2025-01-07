using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TMPro;
using Unity.Properties;
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
    [SerializeField]
    float integrity;

    [CreateProperty]
    public float Integrity
    {
        get { return integrity; }
        set { integrity = value; }
    }

    /// <summary>
    /// infuenced by the player
    /// </summary>
    Human assigned;
    [CreateProperty]
    public Human Assigned
    {
        get { return assigned; }
        set 
        { 
            assigned = value;
            UIUpdate(nameof(Assigned));
        }
    }
    public bool toBeDug;

    /// <summary>
    /// prefab to replace with
    /// </summary>
    public string assetPath;
    

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
    public override GridPos GetPos()
    {
        return new GridPos(
            transform.position.x,
            (transform.position.y - ClickabeObjectFactory.ROCK_OFFSET) / ClickabeObjectFactory.LEVEL_HEIGHT,
            transform.position.z);
    }
    #endregion Basic Operations

    #region Window
    public override InfoWindow OpenWindow()
    {
        InfoWindow info = base.OpenWindow();
        info.Open(this, InfoMode.Rock);
        //info.FillResourceList(info.rock.Q<VisualElement>("Yield"), rockYield);

        return info;
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

    #region Rock actions
    public bool DamageRock(float damage)
    {
        integrity -= damage;
        if (integrity <= 0)
        {
            if (rockYield.ammount.Sum() > 0)
                SceneRefs.objectFactory.CreateAChunk(GetPos(), rockYield);
            MyGrid.UnsetRock(this);
            SceneRefs.objectFactory.CreateRoad(GetPos(), true);
            Destroy(gameObject);

            return true;
        }
        UIUpdate(nameof(Integrity));
        return false;
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

    #endregion
}