using Assets.Scripts.Editor.Columns;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine.UIElements;

namespace Assets.Scripts.Editor.Buildings.SpecialColumns
{
    public class SpecialActions : ColumnActions
    {

        public void AssignChange(ChangeEvent<int> ev)
        {
            int i = ev.target.GetRowIndex();
            ((IAssign)((BuildingWrapper)view.itemsSource[i]).Building).AssignData.AssignLimit.BaseValue = ev.newValue;
            EditorUtility.SetDirty(((BuildingWrapper)view.itemsSource[i]).Building);
        }/*

        
        public void ProdTimeChange(ChangeEvent<int> ev)
        {
            int i = ev.target.GetRowIndex();
            ((IProduction)((BuildingWrapper)view.itemsSource[i]).building).ProdTime = ev.newValue;
            EditorUtility.SetDirty(((BuildingWrapper)view.itemsSource[i]).building);
        }


        public void CanStoreFluidsChange(ChangeEvent<ulong> ev)
        {
            int i = ev.target.GetRowIndex();
            Building prev = ((BuildingWrapper)view.itemsSource[i]).building;
            if (prev != null)
            {
                if (prev is FluidTank tank)
                {
                    if (tank.TypesToStore != ev.newValue)
                        tank.TypesToStore = ev.newValue;
                }
                else if (prev is IStorage storage)
                {
                    if (storage.CanStoreMask != ev.newValue)
                        storage.CanStoreMask = ev.newValue;
                }
                else
                    return;
            }
            EditorUtility.SetDirty(prev);
        }

        
        public void RangeChange(ChangeEvent<int> ev)
        {
            int i = ev.target.GetRowIndex();
            Building prev = ((BuildingWrapper)view.itemsSource[i]).building;
            if (prev != null)
            {
                if (prev is Pub pub)
                {
                    pub.Range = new(ev.newValue);
                }
            }
            EditorUtility.SetDirty(prev);
        }

        
        public void CategoryChange(ChangeEvent<int> ev)
        {
            int i = ev.target.GetRowIndex();
            Building prev = ((BuildingWrapper)view.itemsSource[i]).building;
            if (prev != null)
            {
                int categ = prev.BuildingCateg;
                if (categ != ev.newValue)
                {
                    prev.BuildingCateg = ev.newValue;
                }
            }
        }
        
        
        public void StorageCapacityChanged(ChangeEvent<int> ev)
        {
            int i = ev.target.GetRowIndex();
            if (((BuildingWrapper)view.itemsSource[i]).building != null)
            {
                ((BuildingWrapper)view.itemsSource[i]).building.LocalRes.capacity.BaseValue = ev.newValue;
                EditorUtility.SetDirty(((BuildingWrapper)view.itemsSource[i]).building);
            }
        }

        
        public void FluidCapacityChanged(ChangeEvent<int> ev)
        {
            int i = ev.target.GetRowIndex();
            Building building = ((BuildingWrapper)view.itemsSource[i]).building;
            if (building != null && building is IFluidWork fluidRes)
            {
                fluidRes.StoredFluids.capacity.BaseValue = ev.newValue;
                EditorUtility.SetDirty(building);
            }
        }*/
    }
}
