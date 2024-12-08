using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RadialFillElement : VisualElement, INotifyValueChanged<float>
{
    protected float m_value = float.NaN;
    public void SetValueWithoutNotify(float newValue)
    {
        m_value = newValue;
        radialFill.MarkDirtyRepaint();
    }

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
                    pooled.target = (IEventHandler)this;
                    this.SetValueWithoutNotify(value);
                    this.SendEvent((EventBase)pooled);
                }
            }
            else
            {
                this.SetValueWithoutNotify(value);
            }
        }
    }
    public float width { get; set; }
    public float height { get; set; }
    public Color fillColor { get; set; }
    public float angleOffset { get; set; }
    public string overlayImagePath { get; set; }

    public enum FillDirection
    {
        Clockwise,
        AntiClockwise
    }

    public FillDirection fillDirection { get; set; }
    private float m_overlayImageScale;
    public float overlayImageScale
    {
        get
        {
            m_overlayImageScale = Mathf.Clamp(m_overlayImageScale, 0, 1);
            return m_overlayImageScale;
        }
        set => m_overlayImageScale = value;
    }

    private float radius => (width > height) ? width / 2 : height / 2;

    public VisualElement radialFill;
    public VisualElement overlayImage;

    [System.Obsolete]
    public new class UxmlFactory : UxmlFactory<RadialFillElement, UxmlTraits> { }

    [System.Obsolete]
    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        UxmlFloatAttributeDescription m_value = new UxmlFloatAttributeDescription() { name = "value", defaultValue = 1f };
        UxmlFloatAttributeDescription m_width = new UxmlFloatAttributeDescription() { name = "width", defaultValue = 20f };
        UxmlFloatAttributeDescription m_height = new UxmlFloatAttributeDescription() { name = "height", defaultValue = 20f };
        UxmlFloatAttributeDescription m_angleOffset = new UxmlFloatAttributeDescription() { name = "angle-offset", defaultValue = 0 };
        UxmlColorAttributeDescription m_fillColor = new UxmlColorAttributeDescription() { name = "fill-color", defaultValue = Color.white };
        UxmlStringAttributeDescription m_overlayImagePath = new UxmlStringAttributeDescription() { name = "overlay-image-path", defaultValue = "" };
        UxmlFloatAttributeDescription m_overlayImageScale = new UxmlFloatAttributeDescription() { name = "overlay-image-scale", defaultValue = 1f };
        UxmlEnumAttributeDescription<FillDirection> m_fillDirection = new UxmlEnumAttributeDescription<FillDirection>() { name = "fill-direction", defaultValue = 0 };

        public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
        {
            get { yield break; }

        }
        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);
            var ate = ve as RadialFillElement;

            // Assigning uxml attributes to c# properties
            ate.value = m_value.GetValueFromBag(bag, cc);
            ate.width = m_width.GetValueFromBag(bag, cc);
            ate.height = m_height.GetValueFromBag(bag, cc);
            ate.fillColor = m_fillColor.GetValueFromBag(bag, cc);
            ate.overlayImagePath = m_overlayImagePath.GetValueFromBag(bag, cc);
            ate.overlayImageScale = m_overlayImageScale.GetValueFromBag(bag, cc);
            ate.angleOffset = m_angleOffset.GetValueFromBag(bag, cc);
            ate.fillDirection = m_fillDirection.GetValueFromBag(bag, cc);

            // Creating the hierarchy for the radial fill element
            ate.name = "radial-fill-element";
            ate.Clear();
            VisualElement radialBoundary = new VisualElement() { name = "radial-boundary" };
            radialBoundary.Add(ate.radialFill);
            ate.radialFill.Add(ate.overlayImage);

            ate.radialFill.style.flexGrow = 1;
            ate.overlayImage.style.flexGrow = 1;
            ate.overlayImage.style.scale = new Scale(new Vector2(ate.overlayImageScale, ate.overlayImageScale));
            ate.style.height = ate.height;
            ate.style.width = ate.width;
            radialBoundary.style.height = ate.height;
            radialBoundary.style.width = ate.width;
            radialBoundary.style.overflow = Overflow.Hidden;
            ate.overlayImage.style.backgroundImage = null;
#if UNITY_EDITOR
            Texture2D tex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(ate.overlayImagePath);
            if (tex != null)
            {
                ate.overlayImage.style.backgroundImage = tex;
            }
#endif
            // Angle Offset determines the rotation of the radialFill VE, overlayImage will use the inverse of this
            // rotation so the image remains upright
            ate.radialFill.transform.rotation = Quaternion.Euler(0, 0, ate.angleOffset);
            ate.overlayImage.transform.rotation = Quaternion.Euler(0, 0, -ate.angleOffset);
            ate.Add(radialBoundary);
        }
    }

    public RadialFillElement() : base()
    {
        radialFill = new VisualElement() { name = "radial-fill" };
        overlayImage = new VisualElement() { name = "overlay-image" };
        radialFill.generateVisualContent += OnGenerateVisualContent;
    }

    public void AngleUpdate(ChangeEvent<float> evt)
    {
        radialFill?.MarkDirtyRepaint();
    }

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
}