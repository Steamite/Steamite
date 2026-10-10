using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using Assets.Scripts.Base_Assembly.Game.Grid.GridTiles.New;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.LightTransport;

public class MouseClicker : MonoBehaviour
{
    [SerializeField] InputAction clickAction;
    [SerializeField] InputAction breakAction;

    MouseController controller;
    private void Awake()
    {
        enabled = false;
    }
    public void Init(MouseController _controller)
    {
        controller = _controller;
        enabled = true;
    }

    private void OnEnable()
    {
        clickAction.Enable();
        clickAction.performed += ClickAction_performed;

        breakAction.Enable();
        breakAction.performed += BreakAction_performed;
    }

    private void OnDisable()
    {
        clickAction.performed -= ClickAction_performed;
        clickAction.Disable();
        breakAction.performed -= BreakAction_performed;
        breakAction.Disable();
    }


    private void BreakAction_performed(InputAction.CallbackContext obj)
    {
        controller.BreakAction();
    }

    private void ClickAction_performed(InputAction.CallbackContext obj)
    {
        controller.activeMode.DownObject();
    }
}
