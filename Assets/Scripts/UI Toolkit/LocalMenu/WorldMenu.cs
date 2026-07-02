using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LocalMenuUtility
{
    public class WorldMenu : LocalMenu, IUIElement
    {
        public override void Move()
        {
            Vector2 vector = Mouse.current.position.ReadValue();
            vector.x *= Screen.width / 1920;
            vector.y *= Screen.height/ 1080;
            MoveX(vector.x, vector.x);
            MoveY(vector.y);
        }

        public void Open(object data)
        {
            activeObject = data;
            HandleData();
            Move();
        }
    }
}
