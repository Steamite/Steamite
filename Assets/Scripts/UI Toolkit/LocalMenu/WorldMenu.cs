using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace LocalMenuUtility
{
    public class WorldMenu : LocalMenu, IUIElement
    {
        public override void Move()
        {
            Vector2 vector = Mouse.current.position.ReadValue();
            vector.x *= Screen.width / 1920;
            vector.y *= Screen.height / 1080;
            MoveX(vector.x, vector.x);
            MoveY(vector.y);
            Show();
        }

        public void Open(object data)
        {
            if (data == null)
            {
                Close();
                return;
            }
            activeObject = data;
            HandleData();
            Move();
        }
        protected override void HandleData()
        {
            switch (activeObject)
            {
                case GridTile tile:
                    header.text = tile.TileBase.Name;

                    switch (SceneRefs.Overlays.overlay.ActiveOverlay)
                    {
                        case StabilityOverlay stabilityOverlay:
                            secondHeader.style.display = DisplayStyle.Flex;
                            secondHeader.text = $"Stability: {tile.Stability}";
                            break;
                    }
                    costList.style.display = DisplayStyle.None;
                    description.text = "";
                    break;
            }
        }
    }
}
