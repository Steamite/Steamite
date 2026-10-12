using Assets.Scripts.Base_Assembly.Game.Grid.GridTiles.New;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.LightTransport;
using static Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.LevelGridTile;

public abstract partial class GridTilesModeNew : MonoBehaviour
{
    [Header("Cursor")]
    [SerializeField] Texture2D cursor;
    [SerializeField] Vector2 cursorOffset;

    protected MouseController controller;

    /// <summary>
    /// Called when the same mod should be applied again
    /// </summary>
    public bool ToggleMod = true;

    public virtual void InitMode(MouseController _mouseController)
    {
        controller = _mouseController;
    }


    public virtual bool EnterMod()
    {
        ChangeCursor();
        return true;
    }


    public virtual void ExitMod() { }

    void ChangeCursor()
    {
        Cursor.SetCursor(cursor, cursorOffset, CursorMode.ForceSoftware);
    }

    public virtual void EnterObject(int type, int index) { }

    public virtual void ExitObject(int type, int index) { }

    public virtual void DownObject() { }

    public virtual void UpObject() { }

}
