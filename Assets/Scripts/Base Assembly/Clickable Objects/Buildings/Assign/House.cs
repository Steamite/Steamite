using Assets.Scripts.Editor.Buildings.LevelList;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties;
using UnityEngine;
/// <summary>Provides a place to sleep for <see cref="Human"/>s.</summary>
public class House : Building, IAssign
{
    bool hasPub;
    public bool HasPub 
    { 
        get => hasPub; 
        set 
        { 
            hasPub = value;
            foreach (Human human in assignData.Assign)
            {
                if(hasPub)
                    human.SetEfficiencyState(ModType.Pub, 1);
                else
                    human.SetEfficiencyState(ModType.Pub, -1);
            }
        } 
    }

    [CreateProperty] public AssignData AssignData { get => assignData; set => assignData = value; }
    [SerializeField] AssignData assignData;


    #region Deconstruction
    /// <summary>
    /// <inheritdoc/> <br/>
    /// Also sends assigned Humans to the elevator.
    /// </summary>
    protected override void StartDeconstruction()
    {
        ((IAssign)this).ClearHumans();
        base.StartDeconstruction();
    }
    #endregion

    #region UI
    /// <summary>
    /// <inheritdoc/> <br/>
    /// Adds Assign list to <paramref name="toEnable"/>.
    /// </summary>
    /// <inheritdoc/>
    protected override void ToggleInfoComponents(InfoWindow info, Dictionary<string, List<string>> toEnable)
    {
        toEnable.Add("General", new List<string> { "Assign Info" });
        base.ToggleInfoComponents(info, toEnable);
    }

    /// <inheritdoc/>
    public override List<string> GetInfoText()
    {
        List<string> strings = base.GetInfoText();
        strings[0] = $"Can house up to {AssignData.AssignLimit} workers";
        return strings;
    }
    #endregion

    #region Assign
    /// <summary>
    /// <inheritdoc/> <br/>
    /// And it's <see cref="Human.home"/>.
    /// </summary>
    /// <param name="human"><inheritdoc/></param>
    /// <param name="add"><inheritdoc/></param>
    public bool ManageAssigned(Human human, bool add)
    {
        if (add)
        {
            if (!AssignData.AddHuman(human))
                return false;

            human.home = this;
            if(hasPub)
                human.SetEfficiencyState(ModType.Pub, 1);
        }
        else
        {
            AssignData.RemoveHuman(human);
            human.home = null;
            human.SetEfficiencyState(ModType.Pub, 0);
        }
        UIUpdate(nameof(AssignData));
        return true;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns><returns>Returns homeless <see cref="Human"/>s</returns></returns>
    public List<Human> GetUnassigned()
    {
        return SceneRefs.Humans.GetHumans().Where(q => !AssignData.Assign.Contains(q) && q.home == null).ToList();
    }
    #endregion

    #region Saving
    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        clickable ??= new AssignBSave();
        (clickable as AssignBSave).assigned = AssignData.Assign.Select(q => q.id).ToList();
        return base.Save(clickable);
    }

    public override void Load(ClickableObjectSave save)
    {
        base.Load(save);
    }
    #endregion;
}
