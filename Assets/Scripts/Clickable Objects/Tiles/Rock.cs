using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TMPro;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Makes up most of the map, holds valuable resources. <br/>
/// Can be Dug out.
/// </summary>
public class Rock : ClickableObject
{
    #region Variables
    /// <summary>Data about the rock(set).</summary>
    public Resource rockYield;

    /// <summary>Remaining rock integrity.</summary>
    [SerializeField] float integrity;

    /// <summary>Assigned <see cref="Human"/>.</summary>
    Human assigned;

    /// <summary>Is marked to be digged out.</summary>
    public bool toBeDug;

    /// <summary>Prefab to replace.</summary>
    public string assetPath;
    #endregion

    #region Properties
    /// <inheritdoc cref="integrity"/>
    [CreateProperty]
    public float Integrity
    {
        get { return integrity; }
        set { integrity = value; }
    }

    /// <inheritdoc cref="assigned"/>
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
    #endregion

    #region Basic Operations
    /// <summary>Creates a list from all Rocks on the same level.</summary>
    public override void UniqueID() => CreateNewId(transform.parent.GetComponentsInChildren<Rock>().Select(q => q.id).ToList());
    
    /// <inheritdoc/>
    public override GridPos GetPos()
    {
        return new GridPos(
            transform.position.x,
            (transform.position.y - ClickableObjectFactory.ROCK_OFFSET) / ClickableObjectFactory.LEVEL_HEIGHT,
            transform.position.z);
    }
    #endregion Basic Operations

    #region Window
    /// <summary>
    /// <inheritdoc/>
    /// And opens the info window with <see cref="InfoMode.Rock"/>.
    /// </summary>
    /// <returns><inheritdoc/></returns>
    public override InfoWindow OpenWindow()
    {
        InfoWindow info = base.OpenWindow();
        info.Open(this, InfoMode.Rock);
        return info;
    }
    #endregion Window

    #region Saving
    /// <inheritdoc/>
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
        (clickable as RockSave).toBeDug = toBeDug;
        return base.Save(clickable);
    }

    /// <inheritdoc/>
    public override void Load(ClickableObjectSave save)
    {
        integrity = (save as RockSave).integrity;
        toBeDug = (save as RockSave).toBeDug;
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
    /// <summary>
    /// Lowers <see cref="integrity"/>, and if reaches zero the rock is destroyed. <br/>
    /// Updates UI.
    /// </summary>
    /// <param name="damage">Damage to integrity</param>
    /// <returns>If the rock is destroyed.</returns>
    public bool DamageRock(float damage)
    {
        integrity -= damage;
        if (integrity <= 0)
        {
            if (rockYield.ammount.Sum() > 0)
                SceneRefs.objectFactory.CreateAChunk(GetPos(), rockYield, true);
            SceneRefs.objectFactory.CreateRoad(GetPos(), true);
            MyGrid.UnsetRock(this);
            return true;
        }
        UIUpdate(nameof(Integrity));
        return false;
    }

    /// <summary>Colors dirt, based on integrity.</summary>
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