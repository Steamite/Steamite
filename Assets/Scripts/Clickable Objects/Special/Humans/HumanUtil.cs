using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>"Util" class for managing humans.</summary>
public class HumanUtil : MonoBehaviour
{
    #region Variables
    /// <summary>List of all <see cref="Human"/>s in game.</summary>
    [SerializeField] List<Human> humans;
    /// <summary>Efficiency modifiers for all humans.</summary>
    public EfficencyModifiers modifiers;
    /// <summary>Mainly for testing, contains different colors for hats.</summary>
    [SerializeField] List<Material> hatMaterial;
    #endregion

    #region Getters
    /// <returns>Humans without assigned fulltime job.</returns>
    public List<Human> GetPartTime() => humans.Where(q => q.workplace == null).ToList();

    /// <returns>All Humans.</returns>
    public List<Human> GetHumen() => humans;

    /// <returns>Saved <see cref="humans"/>.</returns>
    public HumanSave[] SaveHumans() => humans.Select(q => q.Save() as HumanSave).ToArray();

    #endregion

    #region Loading Levels
    /// <summary>
    /// Loads all Humans.
    /// </summary>
    /// <param name="progress">NOT USED.</param>
    /// <param name="humanSaves">Humans to load.</param>
    /// <param name="humanActivation">Action to link.</param>
    public void LoadHumans(IProgress<int> progress, HumanSave[] humanSaves, ref Action humanActivation, int HumanWeight, ref int currentProg)
    {
        humans = new();
        foreach (HumanSave hSave in humanSaves)
        {
            AddHuman(SceneRefs.ObjectFactory.CreateSavedHuman(hSave), ref humanActivation);
            progress.Report(currentProg += HumanWeight);
        }
    }

    /// <summary>
    /// Adds human to list and links activate action. Used when loading or starting a new.
    /// </summary>
    /// <param name="h">New <see cref="Human"/>.</param>
    /// <param name="action">Action to link to.</param>
    public void AddHuman(Human h, ref Action action)
    {
        action += h.ActivateHuman;
        humans.Add(h);
    }
    #endregion

    public void AddHuman()
    {
        int i = UnityEngine.Random.Range(0, 2);
        Human human = SceneRefs.ObjectFactory.CreateHuman(Elevator.main.GetPos(), hatMaterial[i], i);
        humans.Add(human);
        human.ActivateHuman();
    }

    /// <summary>
    /// Triggered by <see cref="MyGrid.GridChange"/> toggles Human visibility.
    /// </summary>
    /// <param name="currentLevel">Level where workers need to be hidden.</param>
    /// <param name="newLevel">Level where workers need to be shown.</param>
    public void SwitchLevel(int currentLevel, int newLevel)
    {
        humans.Where(q => q?.GetPos().y == currentLevel).ToList().
            ForEach(q => q.gameObject.SetActive(false));
        humans.Where(q => q?.GetPos().y == newLevel).ToList().
            ForEach(q => q.gameObject.SetActive(true));
    }
}
