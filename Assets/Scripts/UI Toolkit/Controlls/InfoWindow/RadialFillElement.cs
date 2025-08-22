using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace InfoWindowElements
{
    /// <summary></summary>
    [UxmlElement]
    public partial class RadialFillElement : VisualElement, INotifyValueChanged<float>, IUIElement
    {
        public enum FillDirection
        {
            Clockwise,
            AntiClockwise
        }

        #region Variables
        float maxValue;
        protected float m_value = float.NaN;


        #region Attributes
        [UxmlAttribute]
        [CreateProperty]
        public float value
        {
            get
            {
                m_value = Mathf.Clamp(m_value, 0, 1);
                return m_value;
            }
            set
            {
                if (EqualityComparer<float>.Default.Equals(this.m_value, value))
                    return;
                if (this.panel != null)
                {
                    using (ChangeEvent<float> pooled = ChangeEvent<float>.GetPooled(this.m_value, value))
                    {
                        pooled.target = this;
                        this.SetValueWithoutNotify(value);
                        this.SendEvent(pooled);
                    }
                }
                else
                {
                    this.SetValueWithoutNotify(value);
                }
            }
        }
        [UxmlAttribute]
        public float width { get; set; }
        [UxmlAttribute]
        public float height { get; set; }
        [UxmlAttribute]
        public Color fillColor { get; set; }
        [UxmlAttribute]
        public float angleOffset { get; set; }
        [UxmlAttribute]
        public string overlayImagePath { get; set; }

        #endregion


        public FillDirection fillDirection { get; set; }
        private float m_overlayImageScale;

        public VisualElement radialFill;
        public VisualElement overlayImage;

        #endregion

        #region Properties
        /// <summary>
        /// Updates the fill value and repaints.
        /// </summary>
        /// <param name="newValue">New fill value.</param>
        public void SetValueWithoutNotify(float newValue)
        {
            m_value = newValue;
            radialFill.MarkDirtyRepaint();
        }

        public float overlayImageScale
        {
            get
            {
                m_overlayImageScale = Mathf.Clamp(m_overlayImageScale, 0, 1);
                return m_overlayImageScale;
            }
            set => m_overlayImageScale = value;
        }

        float radius => (width > height) ? width / 2 : height / 2;
        #endregion

        #region Constructor
        public RadialFillElement()
        {
            name = "radial-fill-element";
            Clear();
            width = 125;
            height = 125;

            radialFill = new VisualElement() { name = "radial-fill" };
            overlayImage = new VisualElement() { name = "overlay-image" };

            style.height = new(new Length(100, LengthUnit.Percent));
            style.width = new(new Length(100, LengthUnit.Percent));
            style.justifyContent = Justify.FlexEnd;
            style.alignSelf = Align.Stretch;
            style.flexGrow = 1;

            #region Boundary
            VisualElement radialBoundary = new VisualElement() { name = "radial-boundary" };
            radialBoundary.style.height = height;
            radialBoundary.style.width = width;
            radialBoundary.Add(radialFill);
            Add(radialBoundary);
            #endregion

            radialFill.Add(overlayImage);
            radialFill.style.rotate = Quaternion.Euler(0, 0, angleOffset);
            radialFill.generateVisualContent += OnGenerateVisualContent;

            overlayImage.style.scale = new Scale(new Vector2(overlayImageScale, overlayImageScale));
            overlayImage.style.backgroundImage = null;
            overlayImage.style.rotate = Quaternion.Euler(0, 0, -angleOffset);

            value = 0.5f;
            fillColor = new(1, 1, 1, 1);
        }
        #endregion

        /// <inheritdoc/>
        public void Open(object data)
        {
            Debug.Log(((IProduction)data).ProdTime);
            maxValue = ((IProduction)data).ProdTime;
            DataBinding binding = BindingUtil.CreateBinding(nameof(IProduction.CurrentTime));
            binding.sourceToUiConverters.AddConverter(
                (ref float time) =>
                {
                    return time / maxValue;
                });
            SceneRefs.InfoWindow.RegisterTempBinding(new(this, nameof(value)), binding, data);
        }

        #region Radial Logic

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mgc"></param>
        public void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            // default draw 1 triangle
            int triCount = 3;
            int indiceCount = 3;
            m_value = Mathf.Clamp(m_value, 0, 360);
            if (m_value * 360 < 240)
            {
                // Draw only 2 triangles
                if (value * 360 > 120)
                {
                    triCount = 4;
                    indiceCount = 6;
                }
            }
            // Draw 3 triangles
            else
            {
                triCount = 4;
                indiceCount = 9;
                if (m_value < 1)
                {
                    triCount = 5;
                    indiceCount = 9;
                }
            }

            // Create our MeshWriteData object, allocate the least amount of vertices and triangle indices required
            MeshWriteData mwd = mgc.Allocate(triCount, indiceCount);
            Vector3 origin = new Vector3((float)width / 2, (float)height / 2, 0);

            float diameter = 4 * radius;
            float degrees = ((m_value * 360) - 90) / Mathf.Rad2Deg;

            //First two vertex are mandatory for 1 triangle
            mwd.SetNextVertex(new Vertex() { position = origin + new Vector3(0 * diameter, 0 * diameter, Vertex.nearZ), tint = fillColor });
            mwd.SetNextVertex(new Vertex() { position = origin + new Vector3(0 * diameter, -1 * diameter, Vertex.nearZ), tint = fillColor });

            float direction = 1;
            if (fillDirection == FillDirection.AntiClockwise)
            {
                direction = -1;
            }

            mwd.SetNextIndex(0);
            mwd.SetNextIndex((fillDirection == FillDirection.AntiClockwise) ? (ushort)2 : (ushort)1);
            if (m_value * 360 <= 120)
            {
                mwd.SetNextVertex(new Vertex() { position = origin + new Vector3(Mathf.Cos(degrees) * diameter * direction, Mathf.Sin(degrees) * diameter, Vertex.nearZ), tint = fillColor });
                mwd.SetNextIndex((fillDirection == FillDirection.AntiClockwise) ? (ushort)1 : (ushort)2);
            }

            if (m_value * 360 > 120 && m_value * 360 <= 240)
            {
                mwd.SetNextVertex(new Vertex() { position = origin + new Vector3(Mathf.Cos(30 / Mathf.Rad2Deg) * diameter * direction, Mathf.Sin(30 / Mathf.Rad2Deg) * diameter, Vertex.nearZ), tint = fillColor });
                mwd.SetNextIndex((fillDirection == FillDirection.AntiClockwise) ? (ushort)1 : (ushort)2);
                mwd.SetNextVertex(new Vertex() { position = origin + new Vector3(Mathf.Cos(degrees) * diameter * direction, Mathf.Sin(degrees) * diameter, Vertex.nearZ), tint = fillColor });
                mwd.SetNextIndex(0);
                mwd.SetNextIndex((fillDirection == FillDirection.AntiClockwise) ? (ushort)3 : (ushort)2);
                mwd.SetNextIndex((fillDirection == FillDirection.AntiClockwise) ? (ushort)2 : (ushort)3);
            }

            if (m_value * 360 > 240)
            {
                mwd.SetNextVertex(new Vertex() { position = origin + new Vector3(Mathf.Cos(30 / Mathf.Rad2Deg) * diameter * direction, Mathf.Sin(30 / Mathf.Rad2Deg) * diameter, Vertex.nearZ), tint = fillColor });
                mwd.SetNextIndex((fillDirection == FillDirection.AntiClockwise) ? (ushort)1 : (ushort)2);
                mwd.SetNextVertex(new Vertex() { position = origin + new Vector3(Mathf.Cos(150 / Mathf.Rad2Deg) * diameter * direction, Mathf.Sin(150 / Mathf.Rad2Deg) * diameter, Vertex.nearZ), tint = fillColor });
                mwd.SetNextIndex(0);
                mwd.SetNextIndex((fillDirection == FillDirection.AntiClockwise) ? (ushort)3 : (ushort)2);
                mwd.SetNextIndex((fillDirection == FillDirection.AntiClockwise) ? (ushort)2 : (ushort)3);

                if (m_value * 360 >= 360)
                {
                    mwd.SetNextIndex(0);
                    mwd.SetNextIndex((fillDirection == FillDirection.AntiClockwise) ? (ushort)1 : (ushort)3);
                    mwd.SetNextIndex((fillDirection == FillDirection.AntiClockwise) ? (ushort)3 : (ushort)1);
                }
                else
                {
                    mwd.SetNextVertex(new Vertex() { position = origin + new Vector3(Mathf.Cos(degrees) * diameter * direction, Mathf.Sin(degrees) * diameter, Vertex.nearZ), tint = fillColor });
                    mwd.SetNextIndex(0);
                    mwd.SetNextIndex((fillDirection == FillDirection.AntiClockwise) ? (ushort)4 : (ushort)3);
                    mwd.SetNextIndex((fillDirection == FillDirection.AntiClockwise) ? (ushort)3 : (ushort)4);
                }
            }
        }
        #endregion

    }
}