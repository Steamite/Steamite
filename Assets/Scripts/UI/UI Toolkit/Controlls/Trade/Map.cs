using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

[UxmlElement]
public partial class Map : VisualElement
{
    VisualElement mapElem;
    VisualElement arrowLeft;
    VisualElement arrowRight;
    VisualElement arrowTop;
    VisualElement arrowBottom;
    
    Texture2D mapImg;
    [UxmlAttribute] Texture2D mapImage 
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

    [UxmlAttribute] InputActionAsset actions;
    [UxmlAttribute][Range(1, 200)] float moveSpeed = 30;
    [UxmlAttribute][Range(1, 200)] float zoomSpeed = 20;
    [UxmlAttribute][Range(0.1f, 20)] float zoomMoveSpeed = 20;


    float baseWidth = 1759;
    float baseHeight = 2048;

    float zoom = 1;
    bool move;

    public void ToggleControls()
    {
        if(actions.enabled)
            actions.Disable();
        else
            actions.Enable();
    }

    public Map() : base()
    {
        mapElem = new();
        mapElem.name = "MapImage";
        mapElem.style.backgroundImage = mapImg;
        mapElem.style.minHeight = baseHeight;
        mapElem.style.minWidth = baseWidth;
        Add(mapElem);

        #region Arrows
        arrowLeft = CreateArrow(new (true, new Length(25, LengthUnit.Pixel)), new(true, new Length(48, LengthUnit.Percent)), 180);
        arrowRight = CreateArrow(new(false, new Length(25, LengthUnit.Pixel)), new(true, new Length(48, LengthUnit.Percent)), 0);

        arrowTop = CreateArrow(new(true, new Length(48, LengthUnit.Percent)), new(true, new Length(25, LengthUnit.Pixel)), 270);
        arrowBottom = CreateArrow(new(true, new Length(48, LengthUnit.Percent)), new(false, new Length(25, LengthUnit.Pixel)), 90);
        #endregion


        RegisterCallback<WheelEvent>(ZoomMap);
        RegisterCallback<MouseMoveEvent>(
            (eve) => 
            {
                if (actions.actionMaps[3].actions[0].inProgress)
                    Move(eve.mouseDelta * moveSpeed * Time.deltaTime * zoom);
            }
        );
    }

    VisualElement CreateArrow(Tuple<bool, StyleLength> horizontal, Tuple<bool, StyleLength> vertical, float rotation)
    {
        VisualElement arrow = new();
        arrow.name = "Arrow";

        if(horizontal.Item1)
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

    void ZoomMap(WheelEvent wheelEvent)
    {
        if (!actions.actionMaps[3].actions[0].inProgress)
        {
            int zoomMod = wheelEvent.delta.y > 0 ? -1: 1;
            float oldZ = zoom;
            zoom = Mathf.Clamp(zoom + (0.1f * zoomMod) * zoomSpeed * Time.deltaTime, resolvedStyle.width / baseWidth, 2);
            
            if(oldZ != zoom)
            {
                mapElem.style.minHeight = (zoom * baseHeight);
                mapElem.style.minWidth = (zoom * baseWidth);

                move = true;
                Move(new Vector2(baseWidth, baseHeight) * (oldZ - zoom)/2);
            }
        }
    }

    void Move(Vector2 moveBy)
    {
        Vector3 position = mapElem.transform.position;
        float clamp;

        clamp = Mathf.Clamp(resolvedStyle.width - mapElem.style.minWidth.value.value - mapElem.resolvedStyle.marginLeft - mapElem.resolvedStyle.marginRight, -5000, 0);
        position.x = Mathf.Clamp(
            position.x + moveBy.x,
            clamp,
            0);
        arrowLeft.style.display = position.x == 0 ? DisplayStyle.None: DisplayStyle.Flex;
        arrowRight.style.display = position.x == clamp ? DisplayStyle.None: DisplayStyle.Flex;

        clamp = Mathf.Clamp(resolvedStyle.height - mapElem.style.minHeight.value.value - mapElem.resolvedStyle.marginTop - mapElem.resolvedStyle.marginBottom, -5000, 0);
        position.y = Mathf.Clamp(
            position.y + moveBy.y,
            clamp,
            0);

        arrowTop.style.display = position.y == 0 ? DisplayStyle.None : DisplayStyle.Flex;
        arrowBottom.style.display = position.y == clamp ? DisplayStyle.None : DisplayStyle.Flex;

        mapElem.transform.position = position;
    }
}
