using UnityEngine.UIElements;

/// <summary>
/// Interface for Monobehaviours that need to be initialized and interact with 
/// </summary>
public interface IToolkitController
{
    public void Init(VisualElement root);
}
