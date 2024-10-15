using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using System.Threading.Tasks;

public class WorkerAssign : MonoBehaviour
{
    [SerializeField] GameObject buttonPrefab;
    Transform humans;
    public AssignBuilding _building;

    public void FillStart(AssignBuilding _build)
    {
        _building = _build;
        humans = MyGrid.sceneReferences.humans.transform;
        Transform ContentWorkers = transform.GetChild(0).GetChild(0).GetChild(0).transform; // content for assigned
        Transform ContentAssign = transform.GetChild(1).GetChild(0).GetChild(0).transform; // content for unassigned

        List<Human> humen;
        if (_build.GetComponent<ProductionBuilding>())
        {
            // create buttons for humans without workplaces
            humen = humans.GetChild(0).GetComponentsInChildren<Human>().ToList();
        }
        else
        {
            // create buttons for humans without homes
            humen = humans.GetChild(0).GetComponentsInChildren<Human>().ToList();
            humen.AddRange(humans.GetChild(1).GetComponentsInChildren<Human>().ToList());
            humen.RemoveAll(q => _build.assigned.Contains(q) || q.home != null);
        }
        // create buttons for assigned humans
        Fill(ContentWorkers, _build.assigned, buttonPrefab);
        // create buttons for unassigned humans
        Fill(ContentAssign, humen, buttonPrefab);
        gameObject.SetActive(true);
    }

    public void Fill(Transform listView, List<Human> humans, GameObject pref)
    {
        List<string> rendered = new();
        for (int i = 0; i < listView.childCount; i++)
        {
            rendered.Add(listView.GetChild(i).GetChild(0).GetComponent<TMP_Text>().text);
        }
        List<string> oldItems = rendered.ToList();
        foreach (Human h in humans)
        {
            oldItems.Remove(h.name);
        }
        foreach (Human h in humans)
        {
            int i = rendered.IndexOf(h.name);
            Transform g;
            if (i == -1)
            {
                if (oldItems.Count > 0)
                {
                    g = listView.GetChild(rendered.IndexOf(oldItems[0]));
                    oldItems.RemoveAt(0);
                }
                else
                {
                    g = Instantiate(pref, listView.transform).transform;
                }
                g.GetChild(0).GetComponent<TMP_Text>().text = h.name;
            }
            else
            {
                g = listView.GetChild(i);
                
            }
            g.GetChild(1).GetComponent<TMP_Text>().text = h.specialization.ToString();
            g.GetChild(2).GetComponent<TMP_Text>().text = h.jData.job.ToString();
            g.GetComponent<InfoButtons>().id = h.id;
        }
        foreach(string name in oldItems)
        {
            Destroy(listView.GetChild(rendered.IndexOf(name)).gameObject);
        }
    } // filling the table with human elements
    public void ManageHuman(int id) //adding or removing humans
    {
        bool add = _building.assigned.FindIndex(q => q.id == id) == -1;
        if (add && _building.limit <= _building.assigned.Count)
        {
            print("no space");
            return;
        }
        if (_building.GetComponent<ProductionBuilding>())
        {
            if(add)
            {
                Human human = humans.GetChild(0).GetComponentsInChildren<Human>().Single(q => q.id == id);
                JobData jData = PathFinder.FindPath(
                    new List<ClickableObject>() { _building },
                    human);
                if (jData.interest)
                {
                    _building.assigned.Add(human);
                    human.transform.SetParent(humans.GetChild(1).transform);
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
                }
            }
            else
            {
                Human human = humans.GetChild(1).GetComponentsInChildren<Human>().Single(q => q.id == id);
                human.workplace = null;
                _building.assigned.Remove(human);
                human.transform.SetParent(humans.GetChild(0).transform);
                human.jData.job = JobState.Free;
                human.Idle();
            }
        }
        else if (_building.GetComponent<House>())
        {
            Human human = humans.GetChild(0).GetComponentsInChildren<Human>().Union(humans.GetChild(1).GetComponentsInChildren<Human>()).Single(q => q.id == id);
            if (add)
            {
                human.home = _building.GetComponent<House>();
                _building.assigned.Add(human);
            }
            else
            {
                human.home = null;
                _building.assigned.Remove(human);
            }
            _building.OpenWindow();
        }
        
        FillStart(_building);
    }
}