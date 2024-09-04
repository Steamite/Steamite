using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PipePart : ClickableObject
{
    public Pipe connectedPipe; // not parent but the next pipe
}
