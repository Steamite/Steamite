using System;
using System.Collections.Generic;
using TradeData.Locations;
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

        [UxmlAttribute] public bool useBindings;

        [UxmlAttribute] public bool showMoney = true;
        bool hasMoney;

        #region Constructors
        ///<summary> Do not use from code, this is only for adding the resource list from UI Builder.</summary>
        public DoubleResourceList() : base()
        {
            style.alignContent = Align.Center;
            cost = false;
            useBindings = true;
        }

        public DoubleResourceList(bool _cost, string _name, bool _useBindings = false, bool center = true) : base()
        {
            style.height = 85;
            style.alignContent = Align.Center;
            VisualElement content = this.Q<VisualElement>("unity-content-container");
            if (center)
            {
                content.style.flexGrow = 1;
                content.style.justifyContent = Justify.Center;
            }
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

        protected Label noneLabel;

        #region Init
        protected override void SetResWithoutBinding(Resource res)
        {
            List<UIResource> temp = new List<UIResource>();
            if (res is MoneyResource money && showMoney && money.Money > 0)
                temp.Add(new DoubleUIResource(MyRes.Money, +money.Money, ResFluidTypes.Money));
            for (int i = 0; i < res.types.Count; i++)
            {
                temp.Add(new DoubleUIResource(
                    MyRes.resDataSource.GlobalResources[(ResourceType)(object)res.types[i]], res.ammounts[i], res.types[i]));
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
            hasMoney = false;
            if (showMoney && resource is MoneyResource _money)
            {
                if (_money.Money > 0)
                {
                    hasMoney = true;
                    resources.Add(new DoubleUIResource(
                        MyRes.Money,
                        +_money.Money,
                        ResFluidTypes.Money));
                }
            }

            for (int i = 0; i < resource.types.Count; i++)
                resources.Add(new DoubleUIResource(
                    0,
                    resource.ammounts[i],
                    resource.types[i]));

            return propName.CreateBinding();
        }

        protected DataBinding SetupResTypes(Resource resource, string secondPropName, string propName, object data)
        {
            DataBinding mainBind = SetupResTypes(resource, propName);
            DataBinding dataBinding = secondPropName.CreateBinding();
            dataBinding.sourceToUiConverters.AddConverter((ref ModifiableResource secRes) => UpdateSecondResource(secRes));
            SceneRefs.InfoWindow.RegisterTempBinding(new(this, nameof(secondResource)), dataBinding, data);
            return mainBind;
        }

        protected virtual List<UIResource> UpdateSecondResource(Resource resource)
        {
            Debug.Log(resource);
            for (int i = 0; i < resource.types.Count; i++)
                ((DoubleUIResource)resources[i]).secondAmmount = resource.ammounts[i];
            resources = resources;
            return resources;
        }
        #endregion

        #region Convertors
        /// <inheritdoc/>
        protected override List<UIResource> ToUIRes(Resource storage)
        {
            if (hasMoney)
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
        protected override string ConvertString(UIResource resource)
        {
            if (cost)
                return $"{resource.ammount}/{((DoubleUIResource)resource).secondAmmount}";
            else
                return ((DoubleUIResource)resource).secondAmmount > 0 ? $"{((DoubleUIResource)resource).secondAmmount}({resource.ammount})" : "";
        }
        #endregion

        protected override VisualElement MakeItem()
        {
            VisualElement visualElement = base.MakeItem();
            //visualElement.style.alignContent = Align.Center;
            visualElement.style.alignSelf = Align.Center;
            return visualElement;
        }
        protected override VisualElement MakeNoneElement()
        {
            noneLabel = base.MakeNoneElement() as Label;
            return noneLabel;
        }
    }
}

