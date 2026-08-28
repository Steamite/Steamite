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
    [SerializeField] Material mainMapMaterial;

    [SerializeField] Texture2D mainTexture;
    [SerializeField] Texture2D selectedTexture;


    [SerializeField] Texture2D gradientTexture;
    [SerializeField] FilterMode overlayFilter;
    [SerializeField] GridTiles gridTiles;

    public IUIElement worldMenu;

    Image map;

    List<BaseOverlay> overlayModes;
    int activeOverlayIndex = -1;

    public int ActiveOvelayIndex => activeOverlayIndex;
    public BaseOverlay ActiveOverlay => overlayModes[activeOverlayIndex];

    NativeArray<float> mainValueMap;
    NativeArray<float> selectedValueMap;

    public Texture2D GradientTexture => gradientTexture;


    event Action<int, BaseOverlay?> OverlayChanged;

    int gridSize;

    public void AddOverlayChanged(Action<int, BaseOverlay?> action)
    {
        OverlayChanged += action;
        if(activeOverlayIndex > -1)
            action(activeOverlayIndex, overlayModes[activeOverlayIndex]);
    }


    #region Init
    public void AfterLoad()
    {
        gridSize = MyGrid.GridSize;

        mainValueMap = new(gridSize * gridSize, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        selectedValueMap = new(gridSize * gridSize, Allocator.Persistent, NativeArrayOptions.ClearMemory);

        CreateOverlayMap(gridSize);
        mainTexture = CreateTexture(gridSize, "_MainTex",/* mainValueMap,*/ mainMapMaterial);
        selectedTexture = CreateTexture(gridSize, "_SelectedTex", /*selectedValueMap,*/ mainMapMaterial);
        AttachOverlays();

        for (int i = 0; i < MyGrid.NUMBER_OF_LEVELS; i++)
        {
            MyGrid.GetGroundLevelData(i).stability.RegisterChange(() =>
            {
                if(activeOverlayIndex > -1 && 
                    ActiveOverlay.GetType() == typeof(StabilityOverlay))
                    UpdateOverlay();
            });
        }
        MyGrid.AddToGridChange(GridLevelChange);
    }

    private void GridLevelChange(int _, int i)
    {
        UpdateOverlay();
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

        map.material = mainMapMaterial;
        map.gameObject.SetActive(false);
    }

    Texture2D CreateTexture(int size, string name/*, NativeArray<float> map*/, Material material)
    {
        Texture2D tex = new(size, size, TextureFormat.RFloat, false)
        {
            filterMode = overlayFilter
        };
        //map = tex.GetRawTextureData<float>();
        material.SetTexture(name, tex);
        
        
        return tex;
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

            overlay.input.performed += ChangeOverlayAction;
        }
    }
    #endregion Init

    private void ChangeOverlayAction(InputAction.CallbackContext obj)
        => ChangeOverlay(Mathf.RoundToInt(obj.ReadValue<float>()));

    public void ChangeOverlay(int i)
    {
        worldMenu.Open(null);
        if (activeOverlayIndex == i || i == -1)
        {
            map.gameObject.SetActive(false);
            activeOverlayIndex = -1;
            OverlayChanged?.Invoke(activeOverlayIndex, null);

            gridTiles.ChangeSelMode(ControlMode.Nothing);

            return;
        }

        activeOverlayIndex = i;
        UpdateOverlay();
    }



    public void ClearSelectedTiles()
        => SetSelectedTiles(new());

    public void SetSelectedTiles(List<Vector2Int> test)
    {
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                int index = (y * gridSize) + x;
                selectedValueMap[index] = test.Any(q => q.x == x && q.y == y) ? 1 : 0; 
            }
        }

        selectedTexture.SetPixelData(selectedValueMap, 0);
        selectedTexture.Apply();
    }

    void UpdateOverlay()
    {
        if (activeOverlayIndex == -1)
            return;

        // calculate values
        BaseOverlay overlay = overlayModes[activeOverlayIndex];
        overlay.CalculateOverlay(mainValueMap);

        // mark the grid
        Overlay(overlay.gradient);

        OverlayChanged?.Invoke(activeOverlayIndex, overlay);
        if (gridTiles.activeControl != (int)ControlMode.Overlay)
            gridTiles.ChangeSelMode(ControlMode.Overlay);
        else
            mainMapMaterial.SetVector("_MousePos", new(-50, 0, -50));

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
        mainMapMaterial.SetTexture("_GradientTex", gradientTexture);
    }

    public void Overlay(Gradient gradient)
    {
        BakeGradient(gradient);

        mainTexture.SetPixelData(mainValueMap, 0);
        mainTexture.Apply();
    }

    private void OnDestroy()
    {
        mainValueMap.Dispose();
        selectedValueMap.Dispose();
    }

    public List<BaseOverlay> GetButtonOverlayTypes()
    {
        return overlayModes;
    }

    public void ResetOverlayListeners()
    {
        OverlayChanged = null;
    }

    public Vector2Int MouseHitPoint;
    private void Update()
    {
        if (activeOverlayIndex == -1)
            return;
        Plane plane = new(Vector3.up, -2.6f);

        Vector3 vector = Mouse.current.position.value;
        Ray ray = Camera.main.ScreenPointToRay(vector);

        if (plane.Raycast(ray, out float enter))
        {
            Vector3 mouseHitPoint = ray.GetPoint(enter);
            mouseHitPoint.x = MathF.Floor(mouseHitPoint.x + 0.5f);
            mouseHitPoint.z = MathF.Floor(mouseHitPoint.z + 0.5f);
            mainMapMaterial.SetVector("_MousePos", mouseHitPoint);

            if (mouseHitPoint.x >= 0 && mouseHitPoint.x < MyGrid.GridSize &&
                mouseHitPoint.z >= 0 && mouseHitPoint.z < MyGrid.GridSize)
                worldMenu.Open(MyGrid.GetGridTile((int)mouseHitPoint.x, (int)mouseHitPoint.z));
            else
                worldMenu.Open(null);

            MouseHitPoint = new(
                (int)mouseHitPoint.x,
                (int)mouseHitPoint.z);
        }
        else
            MouseHitPoint = new(-1, -1);
    }
}