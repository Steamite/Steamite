using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UIElements;

namespace InfoWindowElements
{
    /// <summary>
    /// <inheritdoc/> <br\>
    /// Adds a second ammount.(for costs)
    /// </summary>
    public class DoubleUIResource : UIResource
    {
        public int secondAmmount;
        public DoubleUIResource(int _ammount, int _secondAmmount, ResourceType _type) : base(_ammount, _type)
        {
            secondAmmount = _secondAmmount;
        }
        public DoubleUIResource(int _ammount, int _secondAmmount) : base(_ammount)
        {
            secondAmmount = _secondAmmount;
        }
    }

    [UxmlElement]
    public partial class DoubleResourceList : ResourceList
    {
        /// <summary>Display as x/y or x (y).</summary>
        [UxmlAttribute] protected bool cost;

        [UxmlAttribute] protected bool useBindings;

        #region Constructors
        ///<summary> Do not use from code, this is only for adding the resource list from UI Builder.</summary>
        public DoubleResourceList() : base()
        {
            //style.marginTop = new(new Length(9, LengthUnit.Percent));
            style.maxWidth = new(new Length(35, LengthUnit.Percent));
            style.alignContent = Align.Center;
            cost = false;
            useBindings = true;
        }

        public DoubleResourceList(bool _cost, string _name, bool _useBindings = false) : base()
        {
            style.marginTop = new(new Length(9, LengthUnit.Percent));
            style.width = new(new Length(35, LengthUnit.Percent));
            style.flexGrow = 0;
            //style.minWidth = new(new Length(35, LengthUnit.Percent));
            style.alignContent = Align.Center;
            cost = _cost;
            name = _name;
            useBindings = _useBindings;
        }
        #endregion

        #region Item Actions
        protected override void BindItem(VisualElement el, int i)
        {
            base.BindItem(el, i);
            if (cost && resources[i].ammount < ((DoubleUIResource)resources[i]).secondAmmount)
                el.Q<Label>("Value").style.color = Color.red;
        }
        #endregion


        #region Init
        /// <inheritdoc/>
        public override void Open(object data)
        {
            DataBinding binding = null;
            switch (data)
            {
                case Building:
                    Building b = (Building)data;
                    if (data is IResourceProduction)
                    {
                        // DEBUG_Binding example binding
                        // Creates a list that's used as itemSource, containg a static resouce and a dynamic binded resource.
                        if (cost)
                            binding = SetupResTypes(((IResourceProduction)b).ProductionCost, nameof(IResourceProduction.InputResource));
                        else
                            binding = SetupResTypes(((IResourceProduction)b).ProductionYield, nameof(Building.LocalRes));
                    }
                    else
                        break;
                    binding.sourceToUiConverters.AddConverter((ref StorageResource storage) => ToUIRes(storage.stored));
                    break;

                case LevelsTab:
                    LevelsTab tab = (LevelsTab)data;
                    Resource resource = tab.LevelData.costs[tab.SelectedLevel];

                    if (cost)
                        binding = SetupResTypes(resource, nameof(ResourceDisplay.GlobalResources));
                    else
                        throw new NotImplementedException();


                    binding.sourceToUiConverters.AddConverter((ref Resource globalStorage) =>
                    {
                        tab.UpdateCostView();
                        return ToUIRes(globalStorage);
                    });
                    data = SceneRefs.BottomBar.GetComponent<ResourceDisplay>();
                    dataSource = data;
                    break;

                case Resource:
                    Resource res = (Resource)data;

                    if (cost && !useBindings)
                    {
                        SetResWithoutBinding(res);
                    }
                    return;
                default:
                    throw new NotImplementedException(data.ToString());
            }
            SceneRefs.infoWindow.RegisterTempBinding(new(this, "resources"), binding, data);
        }


        protected void SetResWithoutBinding(Resource res)
        {
            List<UIResource> temp = new List<UIResource>();
            if (res.capacity > -1)
                temp.Add(new DoubleUIResource(MyRes.Money, res.capacity));
            for (int i = 0; i < res.type.Count; i++)
            {
                temp.Add(new DoubleUIResource(
                    MyRes.resDisplay.GlobalResources[res.type[i]], res.ammount[i], res.type[i]));
            }
            resources = temp;
        }

        /// <summary>
        /// Prepares UI resources for all <see cref="ResourceType"/>s in <paramref name="resource"/>.
        /// </summary>
        /// <param name="resource">Cost resource.</param>
        /// <param name="propName">Name of the datasource property.</param>
        /// <returns></returns>
        protected DataBinding SetupResTypes(Resource resource, string propName)
        {
            resources = new();
            for (int i = 0; i < resource.type.Count; i++)
                resources.Add(new DoubleUIResource(
                        0,
                        resource.ammount[i],
                        resource.type[i]));
            return BindingUtil.CreateBinding(propName);
        }
        #endregion

        #region Convertors
        /// <inheritdoc/>
        protected override List<UIResource> ToUIRes(Resource storage)
        {
            for (int i = 0; i < storage.type.Count; i++)
            {
                int j = resources.FindIndex(q => (ResourceType)q.type == storage.type[i]);
                if (j > -1)
                    resources[j].ammount = storage.ammount[i];
            }
            return resources;
        }

        /// <summary>
        /// Adds a second resource to display.
        /// </summary>
        /// <param name="resource">Data.</param>
        /// <returns>Based on <see cref="cost"/></returns>
        protected override string ConvertString(UIResource resource)
        {
            if (cost)
                return $"{resource.ammount}/{((DoubleUIResource)resource).secondAmmount}";
            else
                return ((DoubleUIResource)resource).secondAmmount > 0 ? $"{((DoubleUIResource)resource).secondAmmount}({resource.ammount})" : "";
        }
        #endregion
    }
}

