using AbstractControls;
using System;
using UnityEngine;
using UnityEngine.UIElements;


namespace TradeWindowElements
{
    [UxmlElement]
    public partial class LocationButton : CustomRadioButton
    {
        public const int elementSize = 150;
        protected Vector2 pos;

        #region Constructors
        public LocationButton()
        {

        }

        public LocationButton(Vector2 _pos, int i) : base("trade-button", i, true)
        {
            pos = _pos;
            style.position = Position.Absolute;
        }
        #endregion

        public virtual void RecalculateLayout(float zoom)
        {
            style.width = elementSize * zoom;
            style.height = elementSize * zoom;

            style.left = (pos.x * zoom) - (style.width.value.value / 2);
            style.top = (pos.y * zoom) - (style.height.value.value / 2);
        }

        protected override void SelectChange(bool UpdateGroup)
        {
            base.SelectChange(UpdateGroup);
        }
    }
}