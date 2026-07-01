using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.UI.Refs___Shortcuts
{
    public static class ShotcutActions
    {
        static bool isShift;
        public static void Shift_canceled(InputAction.CallbackContext obj)
        {
            isShift = false;
        }

        public static void Shift_performed(InputAction.CallbackContext obj)
        {
            isShift = true;
        }

        public static void BuildRotate_performed(InputAction.CallbackContext obj)
        {
            float axis = obj.ReadValue<float>();
            if (SceneRefs.GridTiles.ActiveControl == ControlMode.Build)
            {
                Building building = SceneRefs.GridTiles.BlueprintInstance;
                if (building is Pipe)
                    return;
                if (axis < 0)
                {
                    building.transform.Rotate(new Vector3(0, 90, 0));
                }
                else
                {
                    building.transform.Rotate(new Vector3(0, -90, 0));
                }
                if (building is IFluidWork fluid)
                {
                    fluid.AttachedPipes.ForEach(q => q.RecalculatePipeTransform());
                }
                SceneRefs.GridTiles.Enter();
            }
        }

        public static void Deconstruction_performed(InputAction.CallbackContext obj)
        {
            SceneRefs.GridTiles.ChangeSelMode(ControlMode.Deconstruct);
        }

        public static void Upgrade_performed(InputAction.CallbackContext obj)
        {
            SceneRefs.GridTiles.ChangeSelMode(ControlMode.Upgrade);
        }

        public static void Dig_performed(InputAction.CallbackContext obj)
        {
            SceneRefs.GridTiles.ChangeSelMode(ControlMode.Dig);
        }


        public static void Quests_performed(InputAction.CallbackContext obj)
        {
            UIRefs.Quests.ToggleWindow();
        }

        public static void Trade_performed(InputAction.CallbackContext obj)
        {
            UIRefs.TradingWindow.ToggleWindow();
        }

        public static void Research_performed(InputAction.CallbackContext obj)
        {
            UIRefs.ResearchWindow.ToggleWindow();
        }


        public static void Menu_performed(InputAction.CallbackContext obj)
        {
            UIRefs.PauseMenu.Toggle();
        }
    }
}
