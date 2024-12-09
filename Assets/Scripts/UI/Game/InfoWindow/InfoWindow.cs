using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UIElements;
using System.Linq;

public enum InfoMode
{
    None,
    Building,
    Human,
    Rock,
    Chunk,
    Water,
    Research,
}

public class InfoWindow : MonoBehaviour
{
    [SerializeField] VisualTreeAsset resourceItem;
    [SerializeField] ResourceSkins resourceSkins;


    // universal shorcuts
    public Label header;
    public VisualElement window;

    // main shorcuts
    public VisualElement building;
    public VisualElement human;
    public VisualElement rock;
    public VisualElement chunk;

    // additional shorcuts
    public VisualElement inConstruction;
    public VisualElement constructed;

    InfoMode lastInfo;

    void Awake()
    {
        lastInfo = InfoMode.None;
        window = gameObject.GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("Info-Window");
        window.style.display = DisplayStyle.None;

        header = window.Q<Label>("Header");
        header.parent.Q<Button>("Close").RegisterCallback<ClickEvent>((_) => SceneRefs.gridTiles.DeselectObjects());

        building = window.Q<VisualElement>("Building");
        human = window.Q<VisualElement>("Human");
        rock = window.Q<VisualElement>("Rock");
        chunk = window.Q<VisualElement>("Chunk");

        inConstruction = building.Q<VisualElement>("Construction-View");
        constructed = building.Q<VisualElement>("Constructed");

        constructed.Q<WorkerAssign>("Worker-Assign").Init();
    }

    public void Close()
    {
        window.style.display = DisplayStyle.None;
    }

    public void SwitchMods(InfoMode active)
    {
        if(active != lastInfo)
        {
            switch (lastInfo)
            {
                case InfoMode.None:
                case InfoMode.Water:
                case InfoMode.Research:
                    break;
                case InfoMode.Building:
                    building.style.display = DisplayStyle.None;
                    break;
                case InfoMode.Human:
                    human.style.display = DisplayStyle.None;
                    break;
                case InfoMode.Rock:
                    rock.style.display = DisplayStyle.None;
                    break;
                case InfoMode.Chunk:
                    chunk.style.display = DisplayStyle.None;
                    break;
            }
            lastInfo = active;
        }
        switch (active)
        {
            case InfoMode.None:
            case InfoMode.Water:
            case InfoMode.Research:
                throw new NotImplementedException();
            case InfoMode.Building:
                building.style.display = DisplayStyle.Flex;
                break;
            case InfoMode.Human:
                human.style.display = DisplayStyle.Flex;
                break;
            case InfoMode.Rock:
                rock.style.display = DisplayStyle.Flex;
                break;
            case InfoMode.Chunk:
                chunk.style.display = DisplayStyle.Flex;
                break;
        }
    }
    public void ToggleChildElems(VisualElement element, List<string> toEnable) =>
        element.Children().ToList().ForEach(
            q => q.style.display = toEnable.Contains(q.name)
            ? DisplayStyle.Flex : DisplayStyle.None);

    public void SetAssignButton(bool assign, Transform buttons) // toggles info worker to assigned and back
    {/*
        buttons.parent.GetChild(0).gameObject.SetActive(assign);
        buttons.GetChild(0).GetComponent<Button>().interactable = !assign;
        buttons.GetChild(0).GetChild(0).GetComponent<TMP_Text>().color = !assign ? Color.black : Color.white;
        buttons.GetChild(0).GetChild(0).GetComponent<TMP_Text>().fontStyle = !assign ? FontStyles.Normal : FontStyles.Bold;

        buttons.parent.GetChild(1).gameObject.SetActive(!assign);
        buttons.GetChild(1).GetComponent<Button>().interactable = assign;
        buttons.GetChild(1).GetChild(0).GetComponent<TMP_Text>().color = assign ? Color.black : Color.white;
        buttons.GetChild(1).GetChild(0).GetComponent<TMP_Text>().fontStyle = assign ? FontStyles.Normal : FontStyles.Bold;*/
    }

    public void FillResourceList(VisualElement visualElement, Resource resources)
    {
        List<VisualElement> pool = new();
        List<ResourceType> types = resources.type.ToList();

        foreach(VisualElement item in visualElement.Children().Skip(1))
        {
            ResourceType _resType = (ResourceType)int.Parse(item.name);
            int i = resources.type.IndexOf(_resType);
            if (i > 0)
            {
                item.Q<Label>("Value").text = resources.ammount[i].ToString();
                types.Remove(_resType);
            }
            else 
            { 
                pool.Add(item);
            }
        }

        if(types.Count == 0)
        {
            pool.ForEach(q => visualElement.Remove(q));
        }
        else
        {
            while (types.Count > 0)
            {
                if(pool.Count > 0)
                {
                    FillResItem(pool[0], resources.ammount[resources.type.IndexOf(types[0])], types[0]);
                    pool.RemoveAt(0);
                }
                else
                {
                    resourceItem.CloneTree(visualElement);
                    FillResItem(visualElement.ElementAt(visualElement.childCount-1), resources.ammount[resources.type.IndexOf(types[0])], types[0]);
                }
                types.RemoveAt(0);
            }
        }

    }

    void FillResItem(VisualElement item, int value, ResourceType type)
    {
        item.name = ((int)type).ToString();
        item.Q<Label>("Value").text = value.ToString();
        item.Q<VisualElement>("Icon").style.unityBackgroundImageTintColor = resourceSkins.GetResourceColor(type);
    }
    void FillResItem(VisualElement item, int stored, int needed, ResourceType type)
    {
        item.name = ((int)type).ToString();
        item.Q<Label>("Value").text = $"{stored}/{needed}";
        item.Q<VisualElement>("Icon").style.unityBackgroundImageTintColor = resourceSkins.GetResourceColor(type);
    }
    public void FillResourceList(VisualElement visualElement, Resource stored, Resource needed)
    {
        List<VisualElement> pool = new();
        List<ResourceType> types = needed.type.ToList();

        foreach (VisualElement item in visualElement.Children().Skip(1))
        {
            ResourceType _resType = (ResourceType)int.Parse(item.name);
            int i = needed.type.IndexOf(_resType);
            if (i > 0)
            {
                item.Q<Label>("Value").text = $"{stored.ammount[i].ToString()}/{needed.ammount[i].ToString()}";
                types.Remove(_resType);
            }
            else
            {
                pool.Add(item);
            }
        }

        if (types.Count == 0)
        {
            pool.ForEach(q => visualElement.Remove(q));
        }
        else
        {
            while (types.Count > 0)
            {
                int sIndex = stored.type.IndexOf(types[0]);
                if (pool.Count > 0)
                {
                    FillResItem(
                        pool[0], 
                        sIndex == -1 ? 0 : stored.ammount[sIndex], 
                        needed.ammount[needed.type.IndexOf(types[0])], 
                        types[0]);
                    pool.RemoveAt(0);
                }
                else
                {
                    resourceItem.CloneTree(visualElement);
                    FillResItem(
                        visualElement.ElementAt(visualElement.childCount - 1),
                        sIndex == -1 ? 0 : stored.ammount[sIndex], 
                        needed.ammount[needed.type.IndexOf(types[0])], 
                        types[0]);
                }
                types.RemoveAt(0);
            }
        }
    }
}