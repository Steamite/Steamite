using Unity.Properties;

public abstract class TileSource : ClickableObject
{
    [CreateProperty] public bool HasResources { get; protected set; }

    public abstract object RemoveFromSource(int ammount, bool remove);

}