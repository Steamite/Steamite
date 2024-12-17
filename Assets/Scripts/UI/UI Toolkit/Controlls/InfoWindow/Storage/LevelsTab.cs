using UnityEngine;
using UnityEngine.UIElements;


[UxmlElement]
public partial class LevelsTab : Tab
{
    public LevelsTab() : base("Levels")
    {
        style.flexGrow = 1;
        name = "Levels";
    }
}
