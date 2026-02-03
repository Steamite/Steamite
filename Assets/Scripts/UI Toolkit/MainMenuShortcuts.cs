using UnityEngine;
using UnityEngine.InputSystem;

namespace StartMenu
{
    public class MainMenuShortcuts : MonoBehaviour
    {
        [SerializeField] InputAction action;
        [SerializeField] MyMainMenu mainMenu;

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
                }
                else
                {
                    //mainMenu.CloseWindow();
                }
            }
        }
    }
}