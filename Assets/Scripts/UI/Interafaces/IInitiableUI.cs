/// <summary>UI elements that need to be filled after something happens.</summary>
public interface IInitiableUI
{
    public void Init();
}

public interface IInitiableUI<T>
{
    public void Init(T data);
}