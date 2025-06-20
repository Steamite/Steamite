using System;
using System.Collections.Generic;
using Unity.Properties;
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
        [CreateProperty] List<UIResource> secondResource;
        /// <summary>Display as x/y or x (y).</summary>
        [UxmlAttribute] protected bool cost;

        [UxmlAttribute] protected bool useBindings;

        [UxmlAttribute] protected bool showMoney = true;

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
            DataBinding mainBinding = null;
            switch (data)
            {
                case Building:
                    Building b = (Building)data;
                    if (data is IResourceProduction)
                    {
                        // DEBUG_Binding example binding
                        // Creates a list that's used as itemSource, containg a static resouce and a dynamic binded resource.
                        if (cost)
                            mainBinding = SetupResTypes(
                                ((IResourceProduction)b).ProductionCost,
                                nameof(IResourceProduction.ProductionCost),
                                nameof(IResourceProduction.InputResource),
                                data);
                        else
                            mainBinding = SetupResTypes(
                                ((IResourceProduction)b).ProductionYield,
                                nameof(IResourceProduction.ProductionYield),
                                nameof(Building.LocalRes),
                                data);
                        mainBinding.sourceToUiConverters.AddConverter((ref StorageResource storage) => ToUIRes(storage));
                    }
                    else
                    {
                        mainBinding = SetupResTypes(b.Cost, nameof(ResourceDisplay.GlobalResources));
                        mainBinding.sourceToUiConverters.AddConverter((ref MoneyResource storage) => ToUIRes(storage));
                        data = SceneRefs.BottomBar.GetComponent<ResourceDisplay>();
                        dataSource = data;
                        SetBinding(nameof(resources), mainBinding);
                        ((IUpdatable)data).UIUpdate(nameof(ResourceDisplay.GlobalResources));
                        return;
                    }
                    break;

                case LevelsTab:
                    LevelsTab tab = (LevelsTab)data;
                    Resource resource = tab.LevelData.costs[tab.SelectedLevel];

                    if (cost)
                        mainBinding = SetupResTypes(resource, nameof(ResourceDisplay.GlobalResources));
                    else
                        throw new NotImplementedException();


                    mainBinding.sourceToUiConverters.AddConverter((ref Resource globalStorage) =>
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
            SceneRefs.infoWindow.RegisterTempBinding(new(this, "resources"), mainBinding, data);
        }


        protected void SetResWithoutBinding(Resource res)
        {
            List<UIResource> temp = new List<UIResource>();
            if (res is MoneyResource && showMoney && ((MoneyResource)res).Money > -1)
                temp.Add(new DoubleUIResource(MyRes.Money, +((MoneyResource)res).Money));
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
            if (resource is MoneyResource && showMoney)
                resources.Add(new DoubleUIResource(
                    MyRes.Money,
                    +((MoneyResource)resource).Money));
            for (int i = 0; i < resource.type.Count; i++)
                resources.Add(new DoubleUIResource(
                        0,
                        resource.ammount[i],
                        resource.type[i]));

            return BindingUtil.CreateBinding(propName);
        }

        protected DataBinding SetupResTypes(Resource resource, string secondPropName, string propName, object data)
        {
            DataBinding mainBind = SetupResTypes(resource, propName);
            DataBinding dataBinding = BindingUtil.CreateBinding(secondPropName);
            dataBinding.sourceToUiConverters.AddConverter((ref ModifiableResource secRes) => UpdateSecondResource(secRes));
            SceneRefs.infoWindow.RegisterTempBinding(new(this, nameof(secondResource)), dataBinding, data);
            return mainBind;
        }

        protected virtual List<UIResource> UpdateSecondResource(Resource resource)
        {
            Debug.Log(resource);
            for (int i = 0; i < resource.type.Count; i++)
                ((DoubleUIResource)resources[i]).secondAmmount = resource.ammount[i];
            resources = resources;
            return resources;
        }
        #endregion

        #region Convertors
        /// <inheritdoc/>
        protected override List<UIResource> ToUIRes(Resource storage)
        {
            if (storage is MoneyResource)
            {
                int i = resources.FindIndex(q => q.type == null);
                resources[i].ammount = +((MoneyResource)storage).Money;
            }
            for (int i = 0; i < storage.type.Count; i++)
            {
                int j = resources.FindIndex(q => q.type != null && (ResourceType)q.type == storage.type[i]);
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

