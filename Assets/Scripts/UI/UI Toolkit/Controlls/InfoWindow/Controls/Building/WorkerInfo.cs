using System.Collections.Generic;
using System.Linq;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace InfoWindowViews
{
    /// <summary>Control for managing assigned <see cref="Human"/>s.</summary>
    [UxmlElement]
    public partial class AssignInfo : InfoWindowControl
    {
        #region Variables
        /// <summary>Tab title for assigned.</summary>
        [UxmlAttribute] string ASSIGN = "Assigned";
        /// <summary>Tab title for unassigned.</summary>
        [UxmlAttribute] string FREE = "Unassigned";

        /// <summary>Inspected building-</summary>
        IAssign building;
        /// <summary>List of unassigned <see cref="Human"/>.</summary>
        List<Human> unassigned;

        /// <summary>HumanUtil for accesing <see cref="Human"/>.</summary>
        HumanUtil humans;

        /// <summary>Item prefab.</summary>
        [UxmlAttribute] VisualTreeAsset prefab;

        /// <summary>Header label.</summary>
        Label assignLabel;
        /// <summary>Tab view for managment.</summary>
        TabView view;

        string temp;
        [CreateProperty]
        string assignTextCap
        {
            get => temp;
            set
            {
                temp = value;
                assignLabel.text = value;
            }
        }
        #endregion

        #region Builder init
        public AssignInfo()
        {
            style.flexDirection = FlexDirection.Column;

            assignLabel = new("Assigned 0/#");
            assignLabel.style.alignSelf = Align.Center;
            Add(assignLabel);

            view = new();
            view.style.flexGrow = 1;

            Add(view);
            view.Add(InitListView(ASSIGN));
            view.Add(InitListView(FREE));
        }
        /// <summary>
        /// Inits one of the tabs. Links item actions and applies styles.
        /// </summary>
        /// <param name="tabName">New tab name.</param>
        /// <returns>The new ready tab.</returns>
        Tab InitListView(string tabName)
        {
            List<Human> humans = new();
            ListView listView = new(humans); // content for assigned
            listView.name = tabName;
            listView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            listView.selectionType = SelectionType.None;
            listView.style.flexGrow = 1;
            listView.style.backgroundColor = new StyleColor(Color.black);
            listView.style.unityTextAlign = TextAnchor.MiddleCenter;
            listView.makeItem = () =>
            {
                VisualElement visualElement = prefab.CloneTree();
                return visualElement;
            };

            var boo = tabName == FREE;
            listView.bindItem = (el, i) =>
            {
                el.style.backgroundColor = new(StyleKeyword.None);
                Human human = (Human)listView.itemsSource[i];
                el.dataSource = human;
                el.Q<Button>().clickable = new((_) => ManageHuman(((Human)listView.itemsSource[i]).id, boo));

                el.Q<Label>("Name").text = human.objectName;
                el.Q<Label>("Spec").text = human.specialization.ToString();

                DataBinding binding = BindingUtil.CreateBinding(nameof(Human.Job));
                binding.sourceToUiConverters.AddConverter((ref JobData data) => data.job.ToString());
                SceneRefs.InfoWindow.RegisterTempBinding(new(el.Q<Label>("Job"), "text"), binding, human);
            };

            listView.unbindItem = (el, i) =>
            {
                //Debug.LogWarning($"Unassigned: {boo}, {el.Q<Label>("Name").text}");
            };

            Tab tab = new(tabName);
            tab.style.flexGrow = 1;
            tab.Add(listView);
            return tab;
        }

        #endregion

        #region Logic
        /// <inheritdoc/>
        public override void Open(object obj)
        {
            building = (IAssign)obj;
            if (!humans)
            {
                humans = SceneRefs.Humans;
            }

            assignLabel.text = $"Assigned {building.Assigned.Count}/{building.AssignLimit}";
            DataBinding binding = BindingUtil.CreateBinding(nameof(IAssign.Assigned));
            binding.sourceToUiConverters.AddConverter((ref List<Human> assig) => $"Assigned {assig.Count}/{building?.AssignLimit}");
            SceneRefs.InfoWindow.RegisterTempBinding(new(assignLabel, "text"), binding, building);

            binding = BindingUtil.CreateBinding(nameof(IAssign.AssignLimit));
            binding.sourceToUiConverters.AddConverter((ref ModifiableInteger assig) => $"Assigned {building.Assigned.Count}/{assig.currentValue}");
            SceneRefs.InfoWindow.RegisterTempBinding(new(this, nameof(assignTextCap)), binding, building);

            unassigned = building.GetUnassigned();

            // create buttons for assigned humans
            RenderItems(this.Q<ListView>(ASSIGN), building.Assigned);
            // create buttons for unassigned humans
            RenderItems(this.Q<ListView>(FREE), unassigned);
        }

        /// <summary>
        /// Changes the item sources to have the same elements as humans and rebuilds the UI.
        /// </summary>
        /// <param name="listView">List view to change.</param>
        /// <param name="humans">Humans to assign to the listView.</param>
        void RenderItems(ListView listView, List<Human> humans)
        {
            List<Human> rendered = new();
            foreach (Human h in listView.itemsSource)
                rendered.Add(h);

            List<Human> pool = rendered.ToList();
            foreach (Human h in humans)
                pool.Remove(h);

            foreach (Human h in humans)
            {
                int i = rendered.IndexOf(h);
                if (i == -1)
                {
                    if (pool.Count > 0)
                    {
                        i = rendered.IndexOf(pool[0]);
                        listView.itemsSource[i] = h;
                        pool.RemoveAt(0);
                    }
                    else
                    {
                        listView.itemsSource.Add(h);
                    }
                }
            }
            foreach (Human h in pool)
            {
                listView.itemsSource.Remove(h);
            }
            listView.RefreshItems();
        }

        /// <summary>
        /// Action triggered by clicking on the worker buttons.
        /// </summary>
        /// <param name="id">Human <see cref="ClickableObject.id"/>.</param>
        /// <param name="add">Assign if <see langword="true"/>.</param>
        void ManageHuman(int id, bool add) //adding or removing humans
        {
            if (add)
            {
                Human h = unassigned.First(q => q.id == id);
                if (building.ManageAssigned(h, true))
                    unassigned.Remove(h);

            }
            else
            {
                Human h = building.Assigned.First(q => q.id == id);
                if (building.ManageAssigned(h, false))
                    unassigned.Add(h);
            }

            RenderItems(this.Q<ListView>(ASSIGN), building.Assigned);
            RenderItems(this.Q<ListView>(FREE), unassigned);
        }
        #endregion
    }
}