using Assets.Scripts.Editor.Buildings.LevelList;
using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

[Serializable]
public struct AssignData
{
    public ModifiableInteger AssignLimit;
    
    public int AssignNumber { get; private set; }
    public List<Human> Assign { get; private set; }

    public void SetLimit(int limit)
    {
        if (AssignLimit == null)
            AssignLimit = new(limit);
        else
            AssignLimit.BaseValue = limit;
    }

    public bool AddHuman(Human human)
    {
        if (AssignNumber == AssignLimit.currentValue)
            return false;
        Assign[AssignNumber] = human;
        AssignNumber++;
        return true;
    }

    public void RemoveHuman(Human human)
    {
        int i;
        for (i = 0; i < AssignNumber; i++)
        {
            if (Assign[i].id == human.id)
                break;
        }
        Assign[i] = null;
        for (; i < AssignNumber; i++)
        {
            Assign[i] = Assign[i + 1];
        }
        AssignNumber--;
    }

    public void RemoveAll()
    {
        Assign.Clear();
        AssignNumber = 0;
    }
}

/// <summary>
/// Default implementation for Workplaces.
/// Can be ovewriten for Houses, or other similliar actions.
/// </summary>
public interface IAssign
{
    [CreateProperty] public AssignData AssignData { get; set; }

    #region Assigment
    /// <summary>
    /// Assigns or unassigns the <paramref name="human"/>.
    /// </summary>
    /// <param name="human">To modify.</param>
    /// <param name="add">Add or remove.</param>
    /// <returns>True if succesful, false if operation failed</returns>
    public bool ManageAssigned(Human human, bool add)
    {
        if (add)
        {
            if (AssignData.AddHuman(human))
            {
                if (human.SetWorkplace(this))
                    return true;

                AssignData.RemoveHuman(human);
            }
            return false;
        }
        else
        {
            human.RemoveWorkplace();
            AssignData.RemoveHuman(human);
        }
        ((IUpdatable)this).UIUpdate(nameof(AssignData));
        return true;
    }


    /// <summary>
    /// Returns humans that are not assigned in the buildings.
    /// </summary>
    /// <returns><see cref="NotImplementedException"/> </returns>
    public List<Human> GetUnassigned()
    {
        return SceneRefs.Humans.GetPartTime();
    }

    public void ClearHumans()
    {
        AssignData.RemoveAll();
    }
    #endregion
}
