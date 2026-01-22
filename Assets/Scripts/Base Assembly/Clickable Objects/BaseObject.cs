using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace ClickableObjects
{
    public class BaseObject //: IUpdatable, INameChangable
    {
        /*
        GridPos Pos;
        /// <summary>Bind event for updating.</summary>
        [HideInInspector]
        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

        public string objectName;
        public string Name { get => objectName; set => objectName = value; }

        /// <summary>ID is a unique identifier for each group of objects.</summary>
        public int id = -1;
        #region Object Operations

        /// <summary>
        /// Compares by <see cref="id"/> and name.
        /// </summary>
        /// <param name="obj">Object to compare.</param>
        /// <returns>Comparison result.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            else if (((BaseObject)obj).id == id)
                if (((BaseObject)obj).objectName == objectName)
                    return true;
            return false;
        }

        public override int GetHashCode() => base.GetHashCode();


        /// <summary>
        /// Must be called after updating a bindable parameter. <br/>
        /// Has effect only if there is an active binding. <br/>
        /// Add <b>[CreateProperty]</b> to mark it.
        /// </summary>
        /// <param name="property">Name of the property, not field.</param>
        public void UIUpdate(string property = "")
        {
            propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
        }

        #endregion


        #region Basic Operations
        /// <summary>Should create a unique id for a new object in a list.</summary>
        public virtual void UniqueID() => id = -1;

        public override string ToString()
        {
            return $"{objectName}: {id}";
        }
        /// <summary>
        /// Calculates position on the grid.
        /// </summary>
        /// <returns>Calculated position.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual GridPos GetPos() => throw new NotImplementedException();
        /// <summary>
        /// Fills the <paramref name="jobSave"/> with object id and type.
        /// </summary>
        /// <exception cref="NotImplementedException">Not Implemented</exception>
        public Tuple<int, JobSave.InterestType> GetID()
        {
            switch (this)
            {
                case Pipe:
                    return new(id, JobSave.InterestType.P);
                case Building:
                    return new(id, JobSave.InterestType.B);
                case Rock:
                    return new(id, JobSave.InterestType.R);
                case Chunk:
                    return new(id, JobSave.InterestType.C);
                default:
                    return new(id, JobSave.InterestType.Nothing);
            }
        }
        /// <summary>
        /// Keeps randomly creating new IDs, until it's unique.
        /// </summary>
        /// <param name="clickables">List of taken ids.</param>
        protected void CreateNewId(List<int> clickables) // creates a random int
        {
            do
            {
                id = UnityEngine.Random.Range(0, int.MaxValue);
            } while (clickables.Where(q => q == id).Count() > 0);
            return;
        }

        #endregion Basic Operations

        #region Window
        /// <summary>
        /// Sets the header text and returns info window reference.<br/>
        /// </summary>
        /// <param name="first">Prepare the window.</param>
        /// <returns>Info window reference, if the object is not selected throwns an error.</returns>
        public virtual InfoWindow OpenWindow()
        {
            if (selected)
            {
                InfoWindow info = SceneRefs.InfoWindow;
                info.header.Open(this);
                return info;
            }
            Debug.LogWarning($"Object \"{this}\" not selected, why open window?");
            return null;
        }
        #endregion Window

        #region Saving
        /// <summary>
        /// Recursively calls down and fills the <paramref name="clickable"/>.
        /// </summary>
        /// <param name="clickable">Save data to fill.</param>
        /// <returns>Pointer to <paramref name="clickable"/>.</returns>
        public virtual ClickableObjectSave Save(ClickableObjectSave clickable = null)
        {
            if (clickable == null)
                clickable = new();
            clickable.id = id;
            clickable.objectName = objectName;

            return clickable;
        }
        /// <summary>
        /// Recursively calls down and loads data.
        /// </summary>
        /// <param name="save">Save data to load from.</param>
        public virtual void Load(ClickableObjectSave save)
        {
            id = save.id;
            if (id == -1)
                UniqueID();
            objectName = save.objectName;
        }
        #endregion Saving
        */
    }
}
