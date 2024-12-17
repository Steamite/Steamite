using UnityEngine.UIElements;

public interface IUpdatable : INotifyBindablePropertyChanged
{
    public void UpdateWindow(string property = "");
}