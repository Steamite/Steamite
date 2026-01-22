using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pub : Building, IEffectObject
{
    [SerializeField] ModifiableInteger range = new();
    public ModifiableInteger Range { get => range; set => range = value; }
    public List<Road> effectRoads { get; set; } = new();
    public GridPos effectPos { get; set; }

    public override void FinishBuild()
    {
        base.FinishBuild();
        effectPos = GetPos() + blueprint.moveBy.Rotate(transform.rotation.eulerAngles.y);
        ((IEffectObject)this).UpdateRange(true);
    }

    public override void Load(ClickableObjectSave save)
    {
        base.Load(save);
        effectPos = GetPos() + blueprint.moveBy.Rotate(transform.rotation.eulerAngles.y);
        if (constructed)
            ((IEffectObject)this).UpdateRange(true);
    }

    public override void InitPrefabData()
    {
        base.InitPrefabData();
        ((IModifiable)range).Init();
    }

    public override void DestoyBuilding()
    {
        base.DestoyBuilding();
        ((IEffectObject)this).UpdateRange(false);
    }

    protected override void ToggleInfoComponents(InfoWindow info, Dictionary<string, List<string>> toEnable)
    {
        toEnable.Add("General", new List<string> { "General Info" });
        ((IEffectObject)this).RepaintTiles();
        base.ToggleInfoComponents(info, toEnable);
    }
}