using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

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

        public void AfterLoad()
        {
            hoverMousePosition = default;
            hoverObjectIndex = default;
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

            GetComponent<MouseClicker>().Init(this);

            low = new(Vector3.up, new Vector3(0, lowHeight, 0));
            top = new(Vector3.up, new Vector3(0, topHeight, 0));
        }



        public void Update()
        {
            Vector2 mousePos = Mouse.current.position.value;
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
            for (int i = 0; i < materials.Length; i++)
            {
                if (i == matIndex)
                    continue;
                materials[i].SetFloat("_Range", 0);
            }
            materials[matIndex].SetFloat("_Range", 0.5f + mod);
            materials[matIndex].SetVector("_MousePos", new Vector3(pos.x + 0.5f, 0, pos.y + 0.5f));
            materials[matIndex].SetColor("_HighlightColor", c);
            
            hoverMousePosition = pos;
            hoverObjectType = hoverType;
            hoverObjectIndex = hoverIndex;
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
    }
}
