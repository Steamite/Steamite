

using System.Collections;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

/// <summary>Adds Production Handling.</summary>
public class ResourceProductionBuilding : Building, IAssign, IResourceProduction
{
    #region Variables
    [SerializeField] AssignData assignData;
    [SerializeField][Header("Production")] float productionTime;
    [SerializeField] ModifiableFloat prodSpeed;

    [SerializeField] ModifiableResource resourceCost = new();
    [SerializeField] ModifiableResource resourceYield = new();
    [SerializeField] List<DataAssign> recipesAssigmnet = new();

    #endregion

    #region Properties

    #region Time
    [CreateProperty] public float CurrentTime { get; set; } = 0;
    [CreateProperty] public bool Stoped { get; set; } = false;
    public float ProdTime { get => productionTime; set => productionTime = value; }
    [CreateProperty] public ModifiableFloat ProdSpeed { get => prodSpeed; set => prodSpeed = value; }
    #endregion

    #region Assign
    [CreateProperty] public AssignData AssignData { get => assignData; set => assignData = value; }
    #endregion

    #region Resources
    [CreateProperty] public ProductionStates ProdStates { get; set; } = new();
    [CreateProperty] public StorageResource ProductionStorage { get; set; } = new();
    [CreateProperty] public StorageResource InputResource { get; set; } = new();
    [CreateProperty] public ModifiableResource ResourceCost { get => resourceCost; set => resourceCost = value; }
    [CreateProperty] public ModifiableResource ResourceYield { get => resourceYield; set => resourceYield = value; }
    [CreateProperty] public List<ProductionRecipe> Recipes { get; set; }
    public List<DataAssign> RecipeAsssigment { get => recipesAssigmnet; set => recipesAssigmnet = value; }
    [CreateProperty] public int SelectedRecipe { get; set; }
    #endregion

    #endregion

    #region Window
    /// <summary>
    /// If there's no other component in <see href="toEnable"/> adds "Production", and fills it.
    /// </summary>
    /// <inheritdoc/>
    protected override void ToggleInfoComponents(InfoWindow info, Dictionary<string, List<string>> toEnable)
    {
        toEnable.TryAdd("General", new List<string> { "Production Info", "Assign Info" });
        base.ToggleInfoComponents(info, toEnable);
    }
    #endregion

    #region Saving
    /// <inheritdoc/>
    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        if (clickable == null)
            clickable = new ResProductionBSave();
        (clickable as ResProductionBSave).inputRes = new(InputResource);
        (clickable as ResProductionBSave).productionRes = new(ProductionStorage);
        (clickable as ProductionBSave).currentTime = CurrentTime;
        (clickable as ProductionBSave).ProdStates = ProdStates;
        (clickable as ProductionBSave).selectedRecipe = SelectedRecipe;
        return base.Save(clickable);
    }

    /// <inheritdoc/>
    public override void Load(ClickableObjectSave save)
    {
        InputResource.Load((save as ResProductionBSave).inputRes);
        ProductionStorage.Load((save as ResProductionBSave).productionRes);

        CurrentTime = (save as ProductionBSave).currentTime;
        ProdStates = (save as ProductionBSave).ProdStates;
        SelectedRecipe = (save as ProductionBSave).selectedRecipe;
        base.Load(save);
    }
    #endregion

    #region Storage
    /// <summary>
    /// <inheritdoc/> <br/>
    /// If already constructed use stored resources for production(<see cref="inputResource"/>). 
    /// </summary>
    /// <param name="human"><inheritdoc/></param>
    /// <param name="transferPerTick"><inheritdoc/></param>
    public override void Store(Human human, int transferPerTick)
    {
        if (IsWorking)
        {
            ((IResourceProduction)this).StoreProdResources(human, transferPerTick);
        }
        else
        {
            base.Store(human, transferPerTick);
        }
    }
    public override void Take(Human h, int transferPerTick)
    {
        BaseTake(ProductionStorage, h, transferPerTick, this, nameof(ProductionStorage));

        if (ProductionStorage.HasNoCarriers())
            ((IResourceProduction)this).CancelPickup(false);
    }

    

    /// <inheritdoc/>
    public override void RequestRes(Resource request, Human human, StorageRequestType mod)
    {
        StorageResource storage = null;
        if (!IsWorking)
            storage = LocalRes;
        else if(mod == StorageRequestType.Store)
            storage = InputResource;
        else
            storage = ProductionStorage;

        storage.AddRequest(request, human, mod);
    }

    /// <inheritdoc/>
    public override void TryLink(Human h)
    {
        InputResource.LinkHuman(h);
        ProductionStorage.LinkHuman(h);
        base.TryLink(h);
    }
    #endregion

    #region Construction & Deconstruction

    /// <summary>
    /// <inheritdoc/>
    /// And requests resources for production.
    /// </summary>
    protected override void FinishBuild()
    {
        base.FinishBuild();
        ((IResourceProduction)this).LoadRecipes(true, SceneRefs.ObjectFactory.recipeData);
    }

    protected override void StartDeconstruction()
    {
        JobQueue queue = SceneRefs.JobQueue;
        // Remove assigned workers
        ((IAssign)this).ClearHumans();

        ((IResourceProduction)this).CancelPickup(false); // removed by base class
        ((IResourceProduction)this).CancelSupply(true);

        base.StartDeconstruction();
    }

    /// <summary>
    /// <inheritdoc/> <br/>
    /// And full <see cref="inputResource"/>.
    /// </summary>
    /// <param name="instantPos"><inheritdoc/></param>
    /// <returns><inheritdoc/></returns>
    public override Chunk Deconstruct(GridPos instantPos)
    {
        ((IResourceProduction)this).CancelSupply(true);
        ((IResourceProduction)this).CancelSupply(true);

        Chunk c = base.Deconstruct(instantPos);
        if (c == null)
            return null;
        if (InputResource.Sum() > 0)
        {
            if (!c)
            {
                c = SceneRefs.ObjectFactory.CreateChunk(instantPos, InputResource, true);
            }
            else
                c.LocalRes.Manage(InputResource, true);
            c.LocalRes.Manage(ProductionStorage, true);
        }
        return c;
    }

    #endregion

    #region
    public override void StartUpgrade()
    {
        ((IResourceProduction)this).CancelPickup(false); //cleared by base class
        ((IResourceProduction)this).CancelSupply(true);
        
        base.StartUpgrade();
    }
    #endregion

    #region Placing
    /// <summary>
    /// <inheritdoc/> <br/>
    /// If constructed returns missing resources for production.
    /// </summary>
    /// <param name="r"><inheritdoc/></param>
    /// <returns><inheritdoc/></returns>
    public override Resource GetDiff(Resource r)
    {
        if (IsWorking)
        {
            Resource cost = new();
            cost.Manage(ResourceCost, true, 2);
            return r.Diff(InputResource.Future(), cost);
        }
        return base.GetDiff(r);
    }

    /// <inheritdoc/>
    public override List<string> GetInfoText()
    {
        List<string> strings = base.GetInfoText();
        strings[0] = $"Can employ up to {AssignData.AssignLimit} workers";
        strings.Insert(1, $"<u>Produces</u>: \n{ResourceYield}");
        if (ResourceCost.Sum() > 0)
            strings[1] += $", from: \n{ResourceCost}";
        return strings;
    }
    #endregion
}