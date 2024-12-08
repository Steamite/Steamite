using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
public class MainMenuShortcuts : MonoBehaviour
{
    [SerializeField] InputAction action;
    [SerializeField] List<MonoBehaviour> windows;

    private void OnEnable()
    {
        action.Enable();
    }
    private void OnDisable()
    {
        action.Disable();
    }

    private void Update()
    {
        if (action.triggered)
        {
            if (ConfirmWindow.window.opened)
            {
                ConfirmWindow.window.Close(false);
                return;
            }
            foreach (IGridMenu item in windows.Cast<IGridMenu>())
            {
                if (item.IsOpen())
                {
                    item.ResetWindow();
                    return;
                }

            }
        }
    }
}
