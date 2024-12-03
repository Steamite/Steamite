using UnityEngine;
using UnityEngine.UIElements;

public interface IGridMenu
{
    public void ResetWindow(ClickEvent _ = null);
    public void UpdateButtonState();
}
