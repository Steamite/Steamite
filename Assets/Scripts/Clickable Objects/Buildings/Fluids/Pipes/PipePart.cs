using UnityEngine.EventSystems;

public class PipePart : ClickableObject
{
    public Pipe connectedPipe; // not parent but the next pipe
    public override void OnPointerEnter(PointerEventData eventData)
    {
        transform.parent.GetComponent<ClickableObject>().OnPointerEnter(eventData);
    }
}
