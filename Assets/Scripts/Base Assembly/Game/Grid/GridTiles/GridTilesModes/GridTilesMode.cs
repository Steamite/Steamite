using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public abstract class GridTilesMode : MonoBehaviour
{
    [Header("Cursor")]
    [SerializeField] Texture2D cursor;
    [SerializeField] Vector2 cursorOffset;

    [Header("Highlight")]
    public Color highlightColor;

    protected GridTiles gridTiles;

    public void Init(GridTiles tile)
        => gridTiles = tile;


    /// <summary>
    /// Called when the same mod should be applied again
    /// </summary>
    public virtual bool ToggleMod() => true;

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


    public virtual void EnterObject(ClickableObject enterObject) { }

    public virtual void ExitObject(ClickableObject exitObject) { }

    public virtual void DownObject() { }

    public virtual void UpObject() { }
}
