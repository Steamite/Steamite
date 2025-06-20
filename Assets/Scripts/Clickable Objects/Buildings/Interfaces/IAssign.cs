using System;
using System.Collections.Generic;
using Unity.Properties;

public interface IAssign
{
    [CreateProperty] public List<Human> Assigned { get; set; }
    /// <summary>Needs to have a field in the class, so it can be serialized.</summary>
    [CreateProperty] public ModifiableInteger AssignLimit { get; set; }

    #region Assigment
    /// <summary>
    /// Assigns or unassigns the <paramref name="human"/>.
    /// </summary>
    /// <param name="human">To modify.</param>
    /// <param name="add">Add or remove.</param>
    /// <returns>True if succesful, false if operation failed</returns>
    public bool ManageAssigned(Human human, bool add);


    /// <summary>
    /// Returns humans that are not assigned in the buildings.
    /// </summary>
    /// <returns><see cref="NotImplementedException"/> </returns>
    public List<Human> GetUnassigned();
    #endregion
}
