using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Properties;
using UnityEngine.UIElements;

public interface IAssign
{
    [CreateProperty] public List<Human> Assigned { get; set; }
    public int assignLimit { get; set; }

    #region Assigment
    /// <summary>
    /// Assigns or unassigns the <paramref name="human"/>.
    /// </summary>
    /// <param name="human">To modify.</param>
    /// <param name="add">Add or remove.</param>
    public void ManageAssigned(Human human, bool add);
    

    /// <summary>
    /// Returns humans that are not assigned in the buildings.
    /// </summary>
    /// <returns><see cref="NotImplementedException"/> </returns>
    public List<Human> GetUnassigned();
    #endregion
}
