using UnityEngine.UIElements;

public interface IUpdatable : INotifyBindablePropertyChanged
{
    public void UIUpdate(string property = "");
}