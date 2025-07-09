using System;
using System.Collections.Generic;
using TradeData.Locations;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace InfoWindowElements
{

    public class DoubleUIFluid : DoubleUIResource<FluidType>
    {
        public DoubleUIFluid(int _ammount, int _secondAmmount) : base(_ammount, _secondAmmount)
        {
        }

        public DoubleUIFluid(int _ammount, int _secondAmmount, FluidType _type) : base(_ammount, _secondAmmount, _type)
        {
        }
    }
    public class DoubleUIRes : DoubleUIResource<ResourceType>
    {
        public DoubleUIRes(int _ammount, int _secondAmmount) : base(_ammount, _secondAmmount)
        {
        }

        public DoubleUIRes(int _ammount, int _secondAmmount, ResourceType _type) : base(_ammount, _secondAmmount, _type)
        {
        }
    }

    /// <summary>
    /// <inheritdoc/> <br\>
    /// Adds a second ammount.(for costs)
    /// </summary>
    public class DoubleUIResource<TEnum> : UIResource<TEnum> where TEnum : Enum
    {
        public int secondAmmount;
        public DoubleUIResource(int _ammount, int _secondAmmount, TEnum _type) : base(_ammount, _type)
        {
            secondAmmount = _secondAmmount;
        }
        public DoubleUIResource(int _ammount, int _secondAmmount) : base(_ammount)
        {
            secondAmmount = _secondAmmount;
        }
    }

    [UxmlElement]
    public partial class DoubleResourceList<T, TEnum> : ResourceList<T, TEnum>
        where T : ResAmmount<TEnum>
        where TEnum : Enum
    {
        [CreateProperty] List<UIResource<TEnum>> secondResource;
        /// <summary>Display as x/y or x (y).</summary>
        [UxmlAttribute] public bool cost;

        [UxmlAttribute] public bool useBindings;

        [UxmlAttribute] public bool showMoney = true;

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
            if (cost && resources[i].ammount < ((DoubleUIResource<TEnum>)resources[i]).secondAmmount)
                el.Q<Label>("Value").style.color = Color.red;
        }
        #endregion

        Label noneLabel;

        #region Init
        /// <inheritdoc/>
        public override void Open(object data)
        {
            DataBinding mainBinding = null;
            switch (data)
            {
                case Building:
                    Building b = (Building)data;
                    if (b.id == -1)
                    {
                        mainBinding = SetupResTypes(b.Cost as T, nameof(ResourceDisplay.GlobalResources));
                        mainBinding.sourceToUiConverters.AddConverter((ref MoneyResource storage) => ToUIRes(storage as T));
                        data = SceneRefs.BottomBar.GetComponent<ResourceDisplay>();
                        dataSource = data;
                        SetBinding(nameof(resources), mainBinding);
                        ((IUpdatable)data).UIUpdate(nameof(ResourceDisplay.GlobalResources));
                        return;
                    }
                    else if (data is IResourceProduction)
                    {
                        // DEBUG_Binding example binding
                        // Creates a list that's used as itemSource, containg a static resouce and a dynamic binded resource.
                        if (cost)
                            mainBinding = SetupResTypes(
                                ((IResourceProduction)b).ResourceCost as T,
                                nameof(IResourceProduction.ResourceCost),
                                nameof(IResourceProduction.InputResource),
                                data);
                        else
                            mainBinding = SetupResTypes(
                                ((IResourceProduction)b).ResourceYield as T,
                                nameof(IResourceProduction.ResourceYield),
                                nameof(Building.LocalRes),
                                data);
                        mainBinding.sourceToUiConverters.AddConverter((ref StorageResource storage) => ToUIRes(storage as T));
                    }
                    else if (data is WaterPump)
                    {
                        if (cost)
                        {
                            noneLabel.dataSource = (data as WaterPump).waterSource;
                            noneLabel.style.height = new Length(50, LengthUnit.Pixel);
                            DataBinding binding = BindingUtil.CreateBinding(nameof(Water.Ammount));
                            binding.sourceToUiConverters.AddConverter((ref int amm) => $"Water Source:\n {amm}/1");
                            SceneRefs.infoWindow.RegisterTempBinding(new BindingContext(noneLabel, "text"), binding, (data as WaterPump).waterSource);
                            return;
                        }
                        else
                        {
                            noneLabel.dataSource = data;
                            DataBinding binding = BindingUtil.CreateBinding(nameof(WaterPump.StoredFluids));
                            binding.sourceToUiConverters.AddConverter((ref Fluid fluid) => $"Water: 2({fluid[FluidType.Water]})");
                            SceneRefs.infoWindow.RegisterTempBinding(new BindingContext(noneLabel, "text"), binding, data);
                            return;
                        }
                    }
                    else
                    {

                        return;
                    }

                    break;

                case LevelInfo:
                    LevelInfo tab = (LevelInfo)data;
                    Resource resource = tab.LevelData.costs[tab.SelectedLevel];

                    if (cost)
                        mainBinding = SetupResTypes(resource as T, nameof(ResourceDisplay.GlobalResources));
                    else
                        throw new NotImplementedException();


                    mainBinding.sourceToUiConverters.AddConverter((ref Resource globalStorage) =>
                    {
                        tab.UpdateCostView();
                        return ToUIRes(globalStorage as T);
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
            List<UIResource<TEnum>> temp = new List<UIResource<TEnum>>();
            if (res is MoneyResource && showMoney && ((MoneyResource)res).Money > -1)
                temp.Add(new DoubleUIResource<TEnum>(MyRes.Money, +((MoneyResource)res).Money));
            for (int i = 0; i < res.types.Count; i++)
            {
                temp.Add(new DoubleUIResource<TEnum>(
                    MyRes.resDisplay.GlobalResources[res.types[i]], res.ammounts[i], (TEnum)(object)res.types[i]));
            }
            resources = temp;
        }

        /// <summary>
        /// Prepares UI resources for all <see cref="ResourceType"/>s in <paramref name="resource"/>.
        /// </summary>
        /// <param name="resource">Cost resource.</param>
        /// <param name="propName">Name of the datasource property.</param>
        /// <returns></returns>
        protected DataBinding SetupResTypes(T resource, string propName)
        {
            resources = new();
            showMoney = false;
            if (resource is MoneyResource _money)
            {
                if (_money.Money > 0)
                {
                    showMoney = true;
                    resources.Add(new DoubleUIResource<TEnum>(
                        MyRes.Money,
                        +_money.Money));
                }
            }

            for (int i = 0; i < resource.types.Count; i++)
                resources.Add(new DoubleUIResource<TEnum>(
                    0,
                    resource.ammounts[i],
                    resource.types[i]));

            return BindingUtil.CreateBinding(propName);
        }

        protected DataBinding SetupResTypes(T resource, string secondPropName, string propName, object data)
        {
            DataBinding mainBind = SetupResTypes(resource, propName);
            DataBinding dataBinding = BindingUtil.CreateBinding(secondPropName);
            dataBinding.sourceToUiConverters.AddConverter((ref ModifiableResource secRes) => UpdateSecondResource(secRes));
            SceneRefs.infoWindow.RegisterTempBinding(new(this, nameof(secondResource)), dataBinding, data);
            return mainBind;
        }

        protected virtual List<UIResource<TEnum>> UpdateSecondResource(Resource resource)
        {
            Debug.Log(resource);
            for (int i = 0; i < resource.types.Count; i++)
                ((DoubleUIResource<TEnum>)resources[i]).secondAmmount = resource.ammounts[i];
            resources = resources;
            return resources;
        }
        #endregion

        #region Convertors
        /// <inheritdoc/>
        protected override List<UIResource<TEnum>> ToUIRes(T storage)
        {
            if (showMoney)
            {
                resources[0].ammount = MyRes.Money;
            }
            for (int i = 0; i < storage.types.Count; i++)
            {
                int j = resources.FindIndex(q => q.type != null && q.type.Equals(storage.types[i]));
                if (j > -1)
                    resources[j].ammount = storage.ammounts[i];
            }
            return resources;
        }

        /// <summary>
        /// Adds a second resource to display.
        /// </summary>
        /// <param name="resource">Data.</param>
        /// <returns>Based on <see cref="cost"/></returns>
        protected override string ConvertString(UIResource<TEnum>resource)
        {
            if (cost)
                return $"{resource.ammount}/{((DoubleUIResource<TEnum>)resource).secondAmmount}";
            else
                return ((DoubleUIResource<TEnum>)resource).secondAmmount > 0 ? $"{((DoubleUIResource<TEnum>)resource).secondAmmount}({resource.ammount})" : "";
        }
        #endregion

        protected override VisualElement MakeNoneElement()
        {
            noneLabel = base.MakeNoneElement() as Label;
            return noneLabel;
        }
    }
}

