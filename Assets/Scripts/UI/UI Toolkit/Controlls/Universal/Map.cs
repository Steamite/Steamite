using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace AbstractControls
{

    /// <summary>Control for the map element in trading, has basic moving and zooming.</summary>
    [UxmlElement]
    public partial class Map : VisualElement
    {

        #region Variables
        #region Elements
        /// <summary>The map image control.</summary>
        protected VisualElement mapElem;
        VisualElement arrowLeft;
        VisualElement arrowRight;
        VisualElement arrowTop;
        VisualElement arrowBottom;
        #endregion

        #region Textures
        Texture2D mapImg;
        [UxmlAttribute]
        Texture2D mapImage
        {
            get => mapImg;
            set
            {
                mapImg = value;
                mapElem.style.backgroundImage = value;
            }
        }
        Texture2D arrowImg;
        [UxmlAttribute]
        Texture2D arrowImage
        {
            get => arrowImg;
            set
            {
                arrowImg = value;
                arrowLeft.style.backgroundImage = value;
                arrowRight.style.backgroundImage = value;
                arrowTop.style.backgroundImage = value;
                arrowBottom.style.backgroundImage = value;
            }
        }
        #endregion

        [UxmlAttribute] InputActionAsset actions;
        /// <summary>Speed for scaled movement.</summary>
        [UxmlAttribute][Range(1, 200)] float moveSpeed = 100;
        /// <summary>Speed for zooming.</summary>
        [UxmlAttribute][Range(1, 200)] float zoomSpeed = 150;

        /// <summary>Image Width.</summary>
        protected float baseWidth = 1855;
        /// <summary>Image Height.</summary>
        protected float baseHeight = 2160;

        /// <summary>Current zoom level.</summary>
        protected float zoom = 1;
        #endregion
        bool ignoreNextDelta = false;
        Rect boundBox;
        float screenScale;
        protected void EnableInput() => actions.FindActionMap("Trade").Enable();
        protected void DisableInput() => actions.FindActionMap("Trade").Disable();



        #region Constructors
        public Map() : base()
        {
            mapElem = new();
            mapElem.name = "MapImage";
            mapElem.style.backgroundImage = mapImg;
            mapElem.style.minHeight = baseHeight;
            mapElem.style.minWidth = baseWidth;
            Add(mapElem);

            #region Arrows
            arrowLeft = CreateArrow(new(true, new Length(25, LengthUnit.Pixel)), new(true, new Length(48, LengthUnit.Percent)), 180);
            arrowRight = CreateArrow(new(false, new Length(25, LengthUnit.Pixel)), new(true, new Length(48, LengthUnit.Percent)), 0);

            arrowTop = CreateArrow(new(true, new Length(48, LengthUnit.Percent)), new(true, new Length(25, LengthUnit.Pixel)), 270);
            arrowBottom = CreateArrow(new(true, new Length(48, LengthUnit.Percent)), new(false, new Length(25, LengthUnit.Pixel)), 90);
            #endregion

            RegisterCallback<WheelEvent>((eve) => ZoomMap(eve));
            RegisterCallback<MouseEnterEvent>((_) =>
            {
                boundBox = worldBound;
                screenScale = Screen.width / 1920f;
            });
            RegisterCallback<MouseMoveEvent>(
                (eve) =>
                {
                    if (actions.actionMaps[3].actions[0].inProgress && !ignoreNextDelta)
                    {
                        Move(eve.mouseDelta * moveSpeed * Time.deltaTime * zoom / Time.timeScale);
                        #region Mouse warp
                        Vector2 newMousePos = eve.mousePosition;
                        if (eve.mouseDelta.x > 0 && eve.mousePosition.x >= boundBox.xMax - 10)
                        {
                            newMousePos.x = boundBox.x;
                        }
                        else if (eve.mouseDelta.x < 0 && newMousePos.x <= boundBox.x + 10)
                        {
                            newMousePos.x = boundBox.xMax;
                        }

                        // Must be reversed
                        // screen.y is up
                        // boundBox.y is down
                        if (eve.mouseDelta.y > 0 && eve.mousePosition.y >= boundBox.yMax - 10)
                        {
                            newMousePos.y = boundBox.height;
                        }
                        else if (eve.mouseDelta.y < 0 && newMousePos.y <= boundBox.y + 10)
                        {
                            newMousePos.y = 0;
                        }

                        if (newMousePos != eve.mousePosition)
                        {
                            if(newMousePos.x != eve.mousePosition.x)
                                newMousePos.y = Mathf.Abs(newMousePos.y - 1080);
                            Mouse.current.WarpCursorPosition(newMousePos * screenScale);
                            ignoreNextDelta = true;
                        }
                        #endregion
                    }
                    else
                        ignoreNextDelta = false;
                }
            );
        }

        /// <summary>
        /// Creates and Adds an arrow indicator based on positional parameters and rotation.
        /// </summary>
        /// <param name="horizontal">bool = left/right; Length = value and unit</param>
        /// <param name="vertical">bool = top/bottom; Length = value and unit</param>
        /// <param name="rotation">Rotation of the arrow (on the Z-axis).</param>
        /// <returns></returns>
        VisualElement CreateArrow(Tuple<bool, StyleLength> horizontal, Tuple<bool, StyleLength> vertical, float rotation)
        {
            VisualElement arrow = new();
            arrow.name = "Arrow";

            if (horizontal.Item1)
                arrow.style.left = horizontal.Item2;
            else
                arrow.style.right = horizontal.Item2;

            if (vertical.Item1)
                arrow.style.top = vertical.Item2;
            else
                arrow.style.bottom = vertical.Item2;

            arrow.transform.rotation = Quaternion.Euler(0, 0, rotation);

            Add(arrow);
            return arrow;
        }
        #endregion

        #region User input
        /// <summary>
        /// Triggered by moving the mouse wheel over the map, zooms on the current view center.
        /// </summary>
        /// <param name="wheelEvent">Scroll event.</param>
        protected virtual bool ZoomMap(WheelEvent wheelEvent)
        {
            if (!actions.actionMaps[3].actions[0].inProgress)
            {
                int zoomMod = wheelEvent.delta.y > 0 ? -1 : 1;
                float oldZ = zoom;
                zoom = Mathf.Clamp(zoom + (0.1f * zoomMod) * zoomSpeed * Time.deltaTime / Time.timeScale, resolvedStyle.width / baseWidth, 2);

                if (oldZ != zoom)
                {
                    mapElem.style.minHeight = (zoom * baseHeight);
                    mapElem.style.minWidth = (zoom * baseWidth);

                    Move(new Vector2(baseWidth, baseHeight) * (oldZ - zoom) / 2);
                    return true;
                }
                return false;
            }
            return false;
        }

        /// <summary>
        /// Moves the <see cref="mapElem"/> by the passed value.
        /// </summary>
        /// <param name="moveBy">Direction and value to move by.</param>
        protected virtual void Move(Vector2 moveBy)
        {
            Vector3 position = mapElem.transform.position;
            float clamp;

            clamp = Mathf.Clamp(resolvedStyle.width - mapElem.style.minWidth.value.value - mapElem.resolvedStyle.marginLeft - mapElem.resolvedStyle.marginRight, -5000, 0);
            position.x = Mathf.Clamp(
                position.x + moveBy.x,
                clamp,
                0);
            arrowLeft.style.display = position.x == 0 ? DisplayStyle.None : DisplayStyle.Flex;
            arrowRight.style.display = position.x == clamp ? DisplayStyle.None : DisplayStyle.Flex;

            clamp = Mathf.Clamp(resolvedStyle.height - mapElem.style.minHeight.value.value - mapElem.resolvedStyle.marginTop - mapElem.resolvedStyle.marginBottom, -5000, 0);
            position.y = Mathf.Clamp(
                position.y + moveBy.y,
                clamp,
                0);

            arrowTop.style.display = position.y == 0 ? DisplayStyle.None : DisplayStyle.Flex;
            arrowBottom.style.display = position.y == clamp ? DisplayStyle.None : DisplayStyle.Flex;

            mapElem.transform.position = position;
            ToolkitUtils.localMenu.Move();
        }
        #endregion
    }
}