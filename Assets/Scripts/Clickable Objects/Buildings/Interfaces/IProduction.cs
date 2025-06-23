public interface IProduction
{
    #region Properties
    /// <summary>Time it takes to finish one production cycle.</summary>
    public float ProdTime { get; set; }

    /// <summary>Current progress.</summary>
    public float CurrentTime { get; set; }

    /// <summary>Manual stop.</summary>
    public bool Stoped { get; set; }

    /// <summary>Multiplies the weight of progress additions.</summary>
    ModifiableFloat ProdSpeed { get; set; }
    #endregion


    #region Production
    /// <summary>
    /// Adds <paramref name="progress"/> to <see cref="currentTime"/>.
    /// </summary>
    /// <param name="progress">Ammount to add.</param>
    public void ProgressProduction(float progress)
    {
        CurrentTime += +ProdSpeed;
        ((IUpdatable)this).UIUpdate(nameof(CurrentTime));
        if (CurrentTime >= ProdTime)
            Product();
    }

    /// <summary>Called when <see cref="currentTime"/> reaches <see cref="prodTime"/>.</summary>
    public void Product();

    /// <summary>
    /// Manual production toggle.
    /// </summary>
    /// <returns>New toggle state.</returns>
    public bool StopProduction()
    {
        Stoped = !Stoped;
        RefreshStatus();
        return Stoped;
    }

    /// <summary>Toggles state indicators.</summary>
    public void RefreshStatus()
    {
        Building building = this as Building;

        if (building.constructed)
        {
            building.transform.GetChild(0).GetChild(0).gameObject.SetActive(Stoped);
        }
    }
    #endregion
}
