using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Base_Assembly.Game.Grid.GridTiles.New
{
    /// <summary>Handles different control states for <see cref="GridTiles"/>.</summary>
    public enum ControlMode
    {
        /// <summary>Basic inspection mode.</summary>
        Nothing,
        /// <summary>Building deconstruction mode.</summary>
        Deconstruct,
        /// <summary>Rock digging mode.(If started on marked Rocks, unmarks insted)</summary>
        Dig,
        /// <summary>Building placement mode.</summary>
        Build,
        Upgrade,

        Overlay
    }
    public class MouseController : MonoBehaviour, IAfterLoad
    {
        GridTilesModeNew[] modes;
        public GridTilesModeNew activeMode;

        public BuildModeNew buildMode;

        public int activeModeIndex;

        public GridWorld world;
        MouseClicker mouseClicker;

        [Header("Mouse Listener")]
        [SerializeField] float topHeight;
        [SerializeField] float lowHeight;

        [SerializeField] float2 lastPos;

        [Header("Hover Temp Data")]
        public int2 hoverWorldPosition;
        public int hoverObjectIndex;
        public TileType hoverObjectType;

        Plane low;
        Plane top;
        public bool isForcedUpdate = true;

        /// <summary>
        /// Grid, Building, Human, ....
        /// </summary>
        [SerializeField] Material[] materials;
        [Header("Clicked Data")]
        public int clickedType;
        public int clickedIndex;

        public void AfterLoad()
        {
            hoverWorldPosition = -1;
            hoverObjectIndex = -1;
            hoverObjectType = default;

            modes = new GridTilesModeNew[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                if (!transform.GetChild(i).TryGetComponent(out modes[i]))
                    continue;
                modes[i].InitMode(this);
            }
            world = SceneRefs.World;

            buildMode = (BuildModeNew)modes[(int)ControlMode.Build];

            activeModeIndex = 0;
            activeMode = modes[0];

            mouseClicker = GetComponent<MouseClicker>();
            mouseClicker.Init(this);

            low = new(Vector3.up, new Vector3(0, lowHeight, 0));
            top = new(Vector3.up, new Vector3(0, topHeight, 0));
            ClearHoverMaterial();
            ClearSelectMaterial();
        }

        static float2 maxDim = new(1920, 1080);

        float distance;
        Ray ray;
        public void Update()
        {
            if (obscured)
                return;
            // maybe move to avoid querrying each frame
            maxDim.x = Screen.width;
            maxDim.y = Screen.height;


            Vector2 mousePos = math.clamp(
                Mouse.current.position.value, 
                0,
                maxDim);
            if (!isForcedUpdate && Mathf.Approximately(mousePos.x, lastPos.x) && Mathf.Approximately(mousePos.y, lastPos.y))
                return;

            lastPos = mousePos;
            isForcedUpdate = false;

            ray = Camera.main.ScreenPointToRay(mousePos);

            if (!top.Raycast(ray, out distance))
                return;

            MouseListen();
        }

        void MouseListen()
        {
            ray = Camera.main.ScreenPointToRay((Vector2)lastPos);

            if (!top.Raycast(ray, out distance))
                return;

            activeMode.MouseTrack(distance, lowHeight, low, ray);
        }


        public void SetMaterial(int2 pos, int matIndex, Color c, TileType hoverType, int hoverIndex, float mod = 0)
        {
            ClearHoverMaterial(matIndex);

            materials[matIndex].SetVector("_HoverRange", new(0.5f + mod, 0.5f + mod));
            materials[matIndex].SetVector("_HoverPosition", new Vector3(pos.x + 0.5f, 0, pos.y + 0.5f));
            materials[matIndex].SetColor("_HoverColor", c);
            
            hoverWorldPosition = pos;
            hoverObjectType = hoverType;
            hoverObjectIndex = hoverIndex;
        }

        public void SetBuildingHighligh(bool hover = true, int renderIndex = -1, Color color = default)
        {
            if (renderIndex != -1)
                renderIndex = SceneRefs.World.WorldBuildings.buildings[renderIndex].renderIndex;
            if (hover)
            {
                ClearHoverMaterial(1);
                materials[1].SetFloat("_HoverIndex", renderIndex);
                materials[1].SetColor("_HoverColor", color);
            }
            else
            {
                ClearSelectMaterial(1);
                materials[1].SetFloat("_SelectionIndex", renderIndex);
                materials[1].SetColor("_SelectionColor", color);
            }
        }


        public void SetSelectionMaterial(int2 pos, int matIndex, Color color, float mod = 0)
        {
            ClearSelectMaterial(matIndex);
            materials[matIndex].SetVector("_SelectionRange", new(0.5f + mod, 0.5f + mod));
            materials[matIndex].SetVector("_SelectionPosition", new Vector3(pos.x + 0.5f, 0, pos.y + 0.5f));
            materials[matIndex].SetColor("_SelectionColor", color);

            hoverWorldPosition = pos;
        }

        public void ClearHoverMaterial(int matIndex = -1)
        {
            for (int i = 0; i < materials.Length; i++)
            {
                if (i == matIndex || i == 1)
                    continue;
                materials[i].SetVector("_HoverRange", default);
            }


            if (matIndex == 1)
                return;
            materials[1].SetFloat("_HoverIndex", -1);
        }
        void ClearSelectMaterial(int matIndex = -1)
        {
            for (int i = 0; i < materials.Length; i++)
            {
                if (i == matIndex || i == 1)
                    continue;
                materials[i].SetVector("_SelectionRange", default);

            }


            if (matIndex == 1)
                return;
            materials[1].SetFloat("_SelectionIndex", -1);
        }

        public void MouseDown()
        {
            if (obscured)
                return;
            activeMode.DownObject();
        }

        public void MouseUp()
        {
            activeMode.UpObject();
        }

        public void BreakAction()
        {
            if (obscured)
                return;

            /*if (multiSelect.Break())
            {*/
                ChangeSelMode(ControlMode.Nothing);
                //activeMode.EnterObject(hoverTileType, hoverMousePosition);
                
            //Drag = false;
            //}
        }


        bool inChange = false;
        

        /// <summary>
        /// Changes the current tool mod, and manages transitions betwean them.
        /// </summary>
        /// <param name="mode"></param>
        public void ChangeSelMode(ControlMode mode)
        {
            if (inChange)
                return;

            inChange = true;
            int i = (int)mode;

            InfoWindowAdapter.Window.Close();

            if (i == activeModeIndex)
            {
                if (activeMode.ToggleMod)
                {
                    inChange = false;
                    ChangeSelMode(ControlMode.Nothing);
                }
                else
                    DeselectObjects();
            }
            else
            {
                activeMode.ExitMod();
                
                DeselectObjects();
                activeModeIndex = i;
                activeMode = modes[i];

                activeMode.EnterMod();
                
                MouseListen();
            }
            inChange = false;
        }

        public void DeselectObjects()
        {
            ClearSelectMaterial();
            clickedType = -1;
            clickedIndex = -1;
        }

        public void ToggleMouse(bool handleGrid)
        {
            enabled = handleGrid;
            mouseClicker.enabled = handleGrid;
            if (handleGrid == false)
            {
                ClearHoverMaterial();
            }
        }

        public bool obscured = false;
        public void ObscureMouse(bool _obscure)
        {
            obscured = _obscure;
            if (_obscure == true)
            {
                ClearHoverMaterial();
            }
        }


        [SerializeField]
        int damageAmmount = 4;
        public int2 clickedWorldPosition;


        [ContextMenu("DamageRock")]
        public void DamageClickedRock(TileType type, int index)
        {
            if(type == TileType.Wall)
                world.DamageRock(index, damageAmmount);
            else if (type == TileType.Road)
                world.MakeRoad(index);

            isForcedUpdate = true;
        }

        public void UpdateTileUI(int pos, int level)
        {
            // only mode one can select elements
            if (activeModeIndex != 0 
                || level != SceneRefs.World.CurrentLevelIndex 
                || pos != clickedIndex)
                return;

            ((NothingNew)modes[0]).RefreshDownedTile();
        }

        #region Shift
        public bool isShiftPressed;
        [SerializeField] InputAction shift;
        private void OnEnable()
        {
            shift.Enable();
            shift.performed += ShiftAction;
            shift.canceled += ShiftAction;
        }


        private void OnDisable()
        {
            shift.Disable();
            shift.performed -= ShiftAction;
            shift.canceled -= ShiftAction;
        }

        private void ShiftAction(InputAction.CallbackContext obj)
            => isShiftPressed = obj.performed;
        #endregion
    }
}
