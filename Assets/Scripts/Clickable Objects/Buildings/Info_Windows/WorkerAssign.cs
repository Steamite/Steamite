using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.UIElements;
using NUnit.Framework;

[UxmlElement]
public partial class WorkerAssign : TabView
{
    const string ASSIGN = "Assigned-List";
    const string FREE = "Free-List";

    public AssignBuilding _building;

    Humans humans;

    List<Human> unassigned;
    [UxmlAttribute]
    VisualTreeAsset prefab;

    public void Init()
    {
        InitListView(ASSIGN);
        InitListView(FREE);
    }

    void InitListView(string s)
    {
        List<Human> humans = new();
        ListView listView = this.Q<ListView>(s); // content for assigned
        listView.itemsSource = humans;
        listView.makeItem = () =>
        {
            VisualElement visualElement = prefab.CloneTree();
            return visualElement;
        };

        var boo = s == "Free-List";

        listView.bindItem = (el, i) =>
        {
            el.Q<Label>("Name").text = ((Human)listView.itemsSource[i]).name;
            el.Q<Label>("Spec").text = ((Human)listView.itemsSource[i]).specialization.ToString();
            el.Q<Label>("Job").text = ((Human)listView.itemsSource[i]).jData.job.ToString();
            el.Q<Button>().clickable = new((_) => ManageHuman(((Human)listView.itemsSource[i]).id, boo));
        };
    }

    
    public void FillStart(AssignBuilding _build)
    {
        _building = _build;
        humans = SceneRefs.humans;
        ListView assignedList = this.Q<ListView>(ASSIGN); // content for assigned

        ListView freeList = this.Q<ListView>(FREE); // content for assigned

        if (_build.GetComponent<ProductionBuilding>())
        {
            // create buttons for humans without workplaces
            unassigned = humans.GetPartTime();
        }
        else
        {
            // create buttons for humans without homes
            unassigned = humans.GetHumen();
            unassigned.RemoveAll(q => _build.assigned.Contains(q) || q.home != null);
        }
        // create buttons for assigned humans
        Fill(assignedList, _build.assigned);
        // create buttons for unassigned humans
        Fill(freeList, unassigned);
    }

    void Fill(ListView listView, List<Human> humans)
    {
        List<Human> rendered = new();
        foreach(Human h in listView.itemsSource)
        {
            rendered.Add(h);
        }

        List<Human> pool = rendered.ToList();
        foreach(Human h in humans)
        {
            pool.Remove(h);
        }

        foreach(Human h in humans)
        {
            int i = rendered.IndexOf(h);
            if (i == -1)
            {
                if(pool.Count > 0)
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
        foreach(Human h in pool)
        {
            listView.itemsSource.Remove(h);
        }
        listView.RefreshItems();
    }

    void ManageHuman(int id, bool add) //adding or removing humans
    {
        if (add && _building.limit <= _building.assigned.Count)
        {
            Debug.LogError("no space");
            return;
        }
        if (_building.GetComponent<ProductionBuilding>())
        {
            if (add)
            {
                Human human = unassigned.First(q => q.id == id);
                JobData jData = PathFinder.FindPath(
                    new List<ClickableObject>() { _building },
                    human);
                if (jData.interest)
                {
                    unassigned.Remove(human);
                    _building.assigned.Add(human);
                    human.transform.SetParent(humans.transform.GetChild(1).transform);
                    human.workplace = _building as ProductionBuilding;
                    human.jData.interest = jData.interest;
                    human.jData.job = JobState.FullTime;
                    if (!human.nightTime)
                    {
                        human.jData.path = jData.path;
                    }
                    human.ChangeAction(HumanActions.Move);
                    human.lookingForAJob = false;
                }
                else
                {
                    Debug.LogError("cant find way here");
                    return;
                }
            }
            else
            {
                Human human = _building.assigned.Single(q => q.id == id);
                _building.assigned.Remove(human);
                human.workplace = null;
                human.transform.SetParent(humans.transform.GetChild(0).transform);
                human.jData.job = JobState.Free;
                human.Idle();
                unassigned.Add(human);
            }
        }
        else if (_building.GetComponent<House>())
        {
            if (add)
            {
                Human human = unassigned.First(q=> q.id == id);
                human.home = _building.GetComponent<House>();
                _building.assigned.Add(human);
            }
            else
            {
                Human human = _building.assigned.First(q => q.id == id);
                _building.assigned.Remove(human);
                unassigned.Add(human);
                human.home = null;
            }
            _building.OpenWindow();
        }

        Fill(this.Q<ListView>(ASSIGN), _building.assigned);
        Fill(this.Q<ListView>(FREE), unassigned);
    }
}