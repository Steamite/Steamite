using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UIElements;

namespace Assets.Scripts.Editor.Columns
{
    public abstract class ColumnActions
    {
        protected MultiColumnListView view;
        public void Init(MultiColumnListView _view)
        {
            view = _view;
        }
    }
}
