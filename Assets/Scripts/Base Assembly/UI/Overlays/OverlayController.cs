using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class OverlayController : MonoBehaviour, IAfterLoad
{
    [SerializeField] Material overlayMapMaterial;

    [SerializeField] Texture2D texture;
    [SerializeField] Texture2D gradientTexture;

    Image map;

    List<BaseOverlay> overlayModes;
    int activeOverlay = -1;

    NativeArray<float> overlayValueMap;
    #region Init
    public void AfterInit()
    {
        int grid = MyGrid.GridSize;

        overlayValueMap = new(grid * grid, Allocator.Persistent, NativeArrayOptions.ClearMemory);

        CreateOverlayMap(grid);
        CreateTexture(grid);
        AttachOverlays();
    }

    void CreateOverlayMap(int size)
    {
        map = new GameObject(
            "map",
            typeof(RectTransform), typeof(Image))
                .GetComponent<Image>();

        map.rectTransform.SetParent(transform, false);
        map.rectTransform.anchoredPosition = new(size / 2, size / 2);
        map.rectTransform.sizeDelta = new(size, size);

        map.material = overlayMapMaterial;
        map.gameObject.SetActive(false);
    }

    void CreateTexture(int size)
    {
        texture = new(size, size, TextureFormat.RFloat, false)
        {
            filterMode = FilterMode.Bilinear
        };
        overlayMapMaterial.SetTexture("_MainTex", texture);
    }

    bool CheckBindings(BaseOverlay overlay, int i, List<string> paths)
    {
        List<string> temp = overlay.input.bindings.Select(q => q.effectivePath).ToList();
        if (temp.Count == 0 || temp.Any(q => string.IsNullOrEmpty(q)))
        {
            Debug.LogWarning($"No binding set for overlay ({i}) {overlay.name}");
            return true;
        }

        foreach (var item in temp)
        {
            if (paths.Contains(item))
            {
                Debug.LogWarning($"Binding already used for another overlay ({item}) {overlay.name}");
                return true;
            }
            else
                paths.Add(item);
        }
        return false;
    }

    void AttachOverlays()
    {
        List<string> paths = new();
        overlayModes = transform.GetComponentsInChildren<BaseOverlay>().ToList();
        for (int i = 0; i < overlayModes.Count; i++)
        {
            BaseOverlay overlay = overlayModes[i];
            overlay.SetInputIndex(i);
            if (CheckBindings(overlay, i, paths))
                continue;

            overlay.input.performed += ChangeOverlay;
        }
    }
    #endregion Init

    private void ChangeOverlay(InputAction.CallbackContext obj)
    {
        int i = Mathf.RoundToInt(obj.ReadValue<float>());
        if (activeOverlay == i)
        {
            map.gameObject.SetActive(false);
            activeOverlay = -1;
            return;
        }

        activeOverlay = i;

        // calculate values
        BaseOverlay overlay = overlayModes[i];
        overlay.Overlay(overlayValueMap);

        // mark the grid
        Overlay(overlay.gradient);

        map.gameObject.SetActive(true);
    }

   
    void BakeGradient(Gradient gradient)
    {
        int resolution = 256; // 256 pixels is plenty for a smooth color ramp

        gradientTexture = new Texture2D(resolution, 1, TextureFormat.RGBA32, false)
        {
            // CRITICAL: Set to Clamp so 0.0 and 1.0 values don't wrap around and bleed colors
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        // 2. Sample the C# Gradient and write to the texture
        Color[] colors = new Color[resolution];
        for (int i = 0; i < resolution; i++)
        {
            // Normalize i to a 0.0 - 1.0 range
            float t = (float)i / (resolution - 1);
            colors[i] = gradient.Evaluate(t);
        }

        gradientTexture.SetPixels(colors);
        gradientTexture.Apply();
        overlayMapMaterial.SetTexture("_GradientTex", gradientTexture);
    }
    public void Overlay(Gradient gradient)
    {
        BakeGradient(gradient);

        texture.SetPixelData(overlayValueMap, 0);
        texture.Apply();
    }
    private void OnDestroy()
    {
        overlayValueMap.Dispose();
    }
}