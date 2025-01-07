using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UIElements;

namespace InfoWindowViews
{

    [UxmlElement]
    public partial class WorkerAssign : VisualElement, IUIElement
    {
        const string ASSIGN = "Assigned";
        const string FREE = "Unassigned";

        AssignBuilding building;
        List<Human> unassigned;

        Humans humans;
        //InfoWindow infoWindow;

        [UxmlAttribute]
        VisualTreeAsset prefab;

        Label label;
        TabView view;

        #region Builder init
        public WorkerAssign()
        {
            style.flexDirection = FlexDirection.Column;

            label = new("Assigned 0/#");
            label.style.alignSelf = Align.Center;
            Add(label);

            view = new();
            view.style.flexGrow = 1;

            Add(view);
            view.Add(InitListView(ASSIGN));
            view.Add(InitListView(FREE));
        }

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
                Human human = (Human)listView.itemsSource[i];
                el.dataSource = human;
                el.Q<Button>().clickable = new((_) => ManageHuman(((Human)listView.itemsSource[i]).id, boo));

                el.Q<Label>("Name").text = human.name;
                el.Q<Label>("Spec").text = human.specialization.ToString();

                DataBinding binding = Util.CreateBinding(nameof(Human.Job));
                binding.sourceToUiConverters.AddConverter((ref JobData data) => data.job.ToString());
                SceneRefs.infoWindow.RegisterTempBinding(new(el.Q<Label>("Job"), "text"), binding, human);
            };

            listView.unbindItem = (el, i) =>
            {
                Debug.LogWarning($"Unassigned: {boo}, {el.Q<Label>("Name").text}");
            };

            Tab tab = new(tabName);
            tab.style.flexGrow = 1;
            tab.Add(listView);
            return tab;
        }

        #endregion

        #region Logic
        public void Fill(object obj)
        {
            building = (AssignBuilding)obj;
            if (!humans)
            {
                humans = SceneRefs.humans;
            }

            label.text = $"Assigned {building.Assigned.Count}/{building.limit}";
            DataBinding binding = Util.CreateBinding(nameof(AssignBuilding.Assigned));
            binding.sourceToUiConverters.AddConverter((ref List<Human> assig) => $"Assigned {assig.Count}/{building?.limit}");
            SceneRefs.infoWindow.RegisterTempBinding(new(label, "text"), binding, building);

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
                building.ManageAssigned(h, true);
                unassigned.Remove(h);
            }
            else
            {
                Human h = building.Assigned.First(q => q.id == id);
                building.ManageAssigned(h, false);
                unassigned.Add(h);
            }

            RenderItems(this.Q<ListView>(ASSIGN), building.Assigned);
            RenderItems(this.Q<ListView>(FREE), unassigned);
        }
        #endregion
    }
}