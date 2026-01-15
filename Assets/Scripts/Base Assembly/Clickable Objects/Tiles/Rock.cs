using System.Data;
using System.Linq;
using Unity.Properties;
using UnityEngine;
using UnityEngine.EventSystems;
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
    public float originalIntegrity;


    /// <summary>Assigned <see cref="Human"/>.</summary>
    Human assigned;

    /// <summary>Is marked to be digged out.</summary>
    public bool toBeDug;

    /// <summary>Data for replacement.</summary>
    public HiddenSave hiddenSave = new();

    bool hidden = true;
    public bool isQuest = false;
    #endregion

    #region Properties
    /// <inheritdoc cref="integrity"/>
    [CreateProperty]
    public float Integrity
    {
        get { return integrity; }
        set
        {
            integrity = value;
        }
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

    #region Mouse Events

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (hidden && SceneRefs.GridTiles.ActiveControl == ControlMode.Nothing)
            return;
        base.OnPointerDown(eventData);

    }
    #endregion

    #region Highlight
    public override void Highlight(Color color)
    {
        GetComponent<MeshRenderer>().materials[^1].SetColor("_EmissionColor", color);
    }
    #endregion

    #region Window
    /// <summary>
    /// <inheritdoc/>
    /// And opens the info window with <see cref="InfoMode.Rock"/>.
    /// </summary>
    /// <returns><inheritdoc/></returns>
    public override InfoWindow OpenWindow()
    {
        if (hidden)
            return null;
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
        if (rockYield?.types.Count == 0)
            (clickable as RockSave).yeild = new();
        else
            (clickable as RockSave).yeild = new(rockYield);
        (clickable as RockSave).integrity = integrity;
        (clickable as RockSave).originalIntegrity = originalIntegrity;
        (clickable as RockSave).toBeDug = toBeDug;
        (clickable as RockSave).hiddenSave = hiddenSave;
        return base.Save(clickable);
    }

    /// <inheritdoc/>
    public override void Load(ClickableObjectSave save)
    {
        integrity = (save as RockSave).integrity;
        originalIntegrity = (save as RockSave).originalIntegrity;
        toBeDug = (save as RockSave).toBeDug;
        rockYield = new((save as RockSave).yeild);
        hiddenSave = (save as RockSave).hiddenSave;
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
    public bool DamageRock(float damage, Human human)
    {
        Integrity -= damage;
        if (Integrity <= 0)
        {
            if (rockYield?.Sum() > 0)
            {
                Chunk chunk = SceneRefs.ObjectFactory.CreateChunk(
                    hiddenSave.assignedType == HiddenType.Nothing 
                        ? GetPos()
                        : human.GetPos(),
                    rockYield, true);
                chunk.transform.GetChild(1).GetComponent<MeshRenderer>().material.color
                    = GetComponent<MeshRenderer>().material.color;
            }
            SceneRefs.ObjectFactory.CreateObjectUnderRock(this);
            SceneRefs.QuestController.DigRock(this);
            MyGrid.UnsetRock(this);
            return true;
        }
        UIUpdate(nameof(Integrity));
        return false;
    }

    /// <summary>Colors dirt, based on integrity.</summary>
    public void ColorWithIntegrity()
    {
        float f;
        switch (originalIntegrity)
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
            case < 11:
                f = 0.4f;
                break;
            default:
                f = 0.2f;
                break;
        }
        gameObject.GetComponent<MeshRenderer>().materials[^1]
            .SetFloat("_Hadrness", f);
    }

    public void Hide()
    {
        foreach (var material in GetComponent<MeshRenderer>().materials)
        {
            material.SetFloat("_isHidden", 1);
            material.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
        }
        hidden = true;
    }

    public void Unhide()
    {
        foreach (var material in GetComponent<MeshRenderer>().materials)
        {
            material.SetFloat("_isHidden", 0);
            material.DisableKeyword("_SPECULARHIGHLIGHTS_OFF");
        }
        hidden = false;
    }
    #endregion
}