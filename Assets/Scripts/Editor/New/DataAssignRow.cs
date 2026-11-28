using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UIElements;

namespace Assets.Scripts.Editor.New
{
    public class DataAssignRow<T_HOLDER, T_CATEG, T_WRAPPER> : BindableElement
        where T_HOLDER : DataHolder<T_CATEG, T_WRAPPER>
        where T_CATEG : DataCategory<T_WRAPPER> 
        where T_WRAPPER : DataObject
    {
        DropdownField categorySelect;
        DropdownField indexSelect;

        public DataAssignRow(T_HOLDER holder)
        {
            Add(categorySelect = new()
            {
                choices = holder.CategoryChoices(),
                bindingPath = nameof(DataAssign.categoryId)
            });
        }

    }
}
