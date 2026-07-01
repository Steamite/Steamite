using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Assets.Scripts.Editor.Buildings.LevelList
{
    [UxmlElement]
    public partial class FieldLevelList<T, T_CONTROL> : PropertyLevelList<T> where T_CONTROL : VisualElement where T : class
    {
        public FieldLevelList() { }

        public FieldLevelList(params string[] _path) : base(_path)
        {
            
        }

        protected override void Init()
        {
            T_CONTROL control = Activator.CreateInstance<T_CONTROL>();
            Add(control);
        }

        protected override void Bind()
        {
            EnforceLevelList(property);

            T_CONTROL control = this[0] as T_CONTROL;

            if (control is ResourceCell cell)
                cell.Open(
                    building.Costs[selectedLevel],
                    building, 
                    typeof(T) == typeof(MoneyResource));

            else if (control is IUIElement elem)
                elem.Open(
                    property.GetArrayElementAtIndex(selectedLevel));
        }
    }
}
