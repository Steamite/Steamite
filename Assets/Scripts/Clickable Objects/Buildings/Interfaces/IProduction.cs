using UnityEngine;

public interface IProduction
{
    #region Properties
    /// <summary>Time it takes to finish one production cycle.</summary>
    public float ProdTime { get; set; }
    /// <summary>Current progress.</summary>
    public float CurrentTime { get; set; }
    /// <summary>Multiplies the weight of progress additions.</summary>
    public int Modifier { get; set; }
    /// <summary>Manual stop.</summary>
    public bool Stoped { get; set; }
    #endregion


    #region Production
    /// <summary>
    /// Adds <paramref name="progress"/> to <see cref="currentTime"/>.
    /// </summary>
    /// <param name="progress">Ammount to add.</param>
    public void ProgressProduction(float progress);

    /// <summary>Called when <see cref="currentTime"/> reaches <see cref="prodTime"/>.</summary>
    public void Product();
    #endregion
}
