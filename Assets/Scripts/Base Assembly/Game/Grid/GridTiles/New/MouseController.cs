using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.PlayerSettings;

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

        public int activeModeIndex;

        public GridWorld world;
        MouseClicker mouseClicker;

        [Header("Mouse Listener")]
        [SerializeField] float topHeight;
        [SerializeField] float lowHeight;

        [SerializeField] float2 lastPos;

        [Header("Hover Temp Data")]
        public int2 hoverMousePosition;
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
            hoverMousePosition = -1;
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

            ChangeSelMode(0);

            mouseClicker = GetComponent<MouseClicker>();
            mouseClicker.Init(this);

            low = new(Vector3.up, new Vector3(0, lowHeight, 0));
            top = new(Vector3.up, new Vector3(0, topHeight, 0));
        }

        static float2 maxDim = new(1920, 1080);

        public void Update()
        {
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

            Ray ray = Camera.main.ScreenPointToRay(mousePos);

            if (!top.Raycast(ray, out float distance))
                return;

            activeMode.MouseTrack(distance, lowHeight, low, ray);
        }

        
        public void SetMaterial(int2 pos, int matIndex, Color c, TileType hoverType, int hoverIndex, float mod = 0)
        {
            ClearMaterial(matIndex);

            materials[matIndex].SetVector("_HoverRange", new(0.5f + mod, 0.5f + mod));
            materials[matIndex].SetVector("_HoverPosition", new Vector3(pos.x + 0.5f, 0, pos.y + 0.5f));
            materials[matIndex].SetColor("_HoverColor", c);
            
            hoverMousePosition = pos;
            hoverObjectType = hoverType;
            hoverObjectIndex = hoverIndex;
        }

        public void SetSelectionMaterial(int2 pos, int matIndex, Color color, float mod = 0)
        {
            ClearMaterialSelect(matIndex);
            materials[matIndex].SetVector("_SelectionRange", new(0.5f + mod, 0.5f + mod));
            materials[matIndex].SetVector("_SelectionPosition", new Vector3(pos.x + 0.5f, 0, pos.y + 0.5f));
            materials[matIndex].SetColor("_SelectionColor", color);

            hoverMousePosition = pos;
        }

        void ClearMaterial(int matIndex = -1)
        {
            for (int i = 0; i < materials.Length; i++)
            {
                if (i == matIndex)
                    continue;
                materials[i].SetVector("_HoverRange", default);
            }
        }
        void ClearMaterialSelect(int matIndex = -1)
        {
            for (int i = 0; i < materials.Length; i++)
            {
                if (i == matIndex)
                    continue;
                materials[i].SetVector("_SelectionRange", default);
            }
        }


        public void BreakAction()
        {
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
            activeModeIndex = i;
            activeMode = modes[i];

            InfoWindowAdapter.Window.Close();
/*
            if (i == activeMode)
            {
                if (ActiveControl.ToggleMod())
                {
                    inChange = false;
                    ChangeSelMode(ControlMode.Nothing);
                }
                else
                    DeselectObjects();
            }
            else
            {
                ActiveControl.ExitMod();
                DeselectObjects();
                activeControl = i;



                if (ActiveControl.EnterMod())
                    Enter();
            }*/
            inChange = false;
        }

        public void DeselectObjects()
        {
            ClearMaterialSelect();
            clickedType = -1;
            clickedIndex = -1;
        }

        public void ToggleMouse(bool handleGrid)
        {
            enabled = handleGrid;
            mouseClicker.enabled = handleGrid;
            if (handleGrid == false)
                ClearMaterial();
        }

        [SerializeField]
        int damageAmmount = 4;
        [ContextMenu("DamageRock")]
        public void DamageClickedRock()
        {
            if (clickedIndex == -1)
                return;

            if (clickedType == 1)
                world.DamageRock(clickedIndex, damageAmmount);
            else if (clickedType == (int)TileType.Road)
                world.MakeRoad(clickedIndex);

            isForcedUpdate = true;
        }
    }
}
