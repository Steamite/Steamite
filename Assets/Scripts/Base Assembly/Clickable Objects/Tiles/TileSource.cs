using Unity.Properties;
using UnityEngine;

public abstract class TileSource : ClickableObject
{
    [CreateProperty] public bool HasResources { get; protected set; }


    [SerializeField] Resource storing = new();
    [CreateProperty] public Resource Storing { get => storing; set => storing = value; }
    public abstract object RemoveFromSource(int ammount, bool remove);

}