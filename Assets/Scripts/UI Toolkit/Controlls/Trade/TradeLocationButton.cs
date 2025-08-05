using AbstractControls;
using UnityEngine;
using UnityEngine.UIElements;

namespace TradeWindowElements
{
    [UxmlElement]
    public partial class TradeLocationButton : LocationButton
    {
        Vector2 basePosition;
        Slider slider;

        public TradeLocationButton() : base() { }

        public TradeLocationButton(Vector2 _pos, int i, Slider _slider, Vector2 _basePosition, CustomRadioButtonGroup group) : base(_pos, i, group)
        {
            slider = _slider;
            basePosition = _basePosition;
        }

        public override void RecalculateLayout(float zoom)
        {
            base.RecalculateLayout(zoom);
            Vector2 locationPos = pos;
            float distance = Vector2.Distance(basePosition, locationPos);

            slider.parent.style.left = (locationPos.x + basePosition.x) / 2 * zoom;
            slider.parent.style.top = (locationPos.y + basePosition.y) / 2 * zoom;

            float width = distance * zoom;
            float height = 50 * zoom;

            slider.style.width = width;
            slider.style.height = height;

            VisualElement dragger = slider.ElementAt(0).ElementAt(0).ElementAt(2);
            width = 30 * zoom;
            dragger.style.width = width;
            dragger.style.height = width;
            dragger.style.top = (height - width) / 2;
        }
    }
}
