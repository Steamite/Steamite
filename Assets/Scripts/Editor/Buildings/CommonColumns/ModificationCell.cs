using System;
using UnityEngine.UIElements;

namespace Assets.Scripts.Editor.Buildings.CommonColumns
{
    public class ModificationCell : VisualElement, IUIElement<BuildingWrapper>
    {
        public ModificationCell() 
        {
            DropdownField field = new();
            field.RegisterValueChangedCallback(ChangeModifier);
        }

        void ChangeModifier(ChangeEvent<string> evt)
        {
            //int i = evt.newValue
            throw new NotImplementedException();
        }

        BuildingWrapper wrapper;

        public void Open(BuildingWrapper wrapper)
        {
            this.wrapper = wrapper;
            
        }


        void Bind(int i)
        {

        }
    }
}