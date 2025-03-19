using UnityEngine.UIElements;

public interface IGridMenu
{
    public bool IsOpen();
    void OpenWindow(ClickEvent _ = null);
    public void CloseWindow(ClickEvent _ = null);
    public void UpdateButtonState();
}
