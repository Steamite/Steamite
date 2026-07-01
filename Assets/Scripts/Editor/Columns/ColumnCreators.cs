using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UIElements;

namespace Assets.Scripts.Editor.Columns
{
    public abstract class ColumnCreators<T> where T : ColumnActions
    {
        public T actions;
        protected MultiColumnListView view;
        public ColumnCreators(MultiColumnListView _view)
        {
            view = _view;

            actions = Activator.CreateInstance<T>();
            actions.Init(_view);
        }

        public abstract void CreateColumns();
    }
}
