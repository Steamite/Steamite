using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Trade : FullscreenWindow
{
    public enum SelectedCateg
    {
        None,
        Colony,
        Trade,
        Outpost,
    }

    [Header("References")]
    public TradeInfo tradeInfo;
    public ColonyInfo colonyInfo;
    public OutpostInfo outpostInfo;
    [SerializeField] Transform colonyTran;
    public TMP_InputField header;

    [Header("Customizables")]
    [SerializeField] public Color unavailableColor;
    [SerializeField] public Color productionColor;
    [SerializeField] public Color availableColor;
    [SerializeField] float expeditionSpeed = 0.13f;

    [Header("Data")]
    public ColonyLocation colonyLocation;
    public List<TradeLocation> tradeLocations;
    public List<TradeExpedition> expeditions;
    public List<Outpost> outposts;

    [Header("Prefabs")]
    [SerializeField] GameObject expeditionSlider;
    [SerializeField] GameObject routeImage;
    [SerializeField] GameObject tradeButton;


    int lastIndex;
    SelectedCateg isLastLocation;

    public override void OpenWindow()
    {
        base.OpenWindow();
        SelectButton(SelectedCateg.Colony, tradeLocations.Count);
        if (expeditions.Count > 0)
        {
            StartCoroutine(MoveTradeRoute());
        }
    }
    public override void CloseWindow()
    {
        base.CloseWindow();
        StopCoroutine(MoveTradeRoute());
    }

    public void NewGame()
    {
        tradeLocations = ((TradeHolder)Resources.Load("Holders/Data/Trade Data")).tradeLocations;
        expeditions = new();
        Init();
    }

    public void LoadGame(TradeSave tradeSave)
    {
        colonyLocation = tradeSave.colonyLocation;
        tradeLocations = tradeSave.tradeLocations;
        expeditions = tradeSave.expeditions;
        outposts = tradeSave.outposts;
        MyRes.money = tradeSave.money;

        Init();

        // assigns sliders
        if (expeditions.Count > 0)
        {
            foreach (TradeExpedition exp in expeditions)
            {
                LoadExpedition(exp);
            }
            SceneRefs.tick.tickAction += MoveTradeRouteProgress;
            StartCoroutine(MoveTradeRoute());
        }
    }

    /// <summary>
    /// The trade window initialization, creates all needed objects.
    /// </summary>
    void Init()
    {
        lastIndex = -2;
        isLastLocation = SelectedCateg.None;
        header.text = colonyLocation.name;

        //PrepBaseLocation(colonyLocation.passiveProductions,0);
        PrepBaseLocation(colonyLocation.stats, 1);

        for (int i = 0; i < colonyLocation.stats[0].currentProduction; i++)
        {
            AddExpeditionSlider();
        }

        // Colony button
        Transform buttonGroupTran = transform.GetChild(0).GetChild(1);
        RectTransform locationButton = Instantiate(tradeButton, buttonGroupTran).GetComponent<RectTransform>();
        locationButton.anchoredPosition = new(colonyLocation.pos.x, colonyLocation.pos.z);

        // Trade buttons
        float scale = gameObject.GetComponent<Canvas>().scaleFactor;
        for (int i = 0; i <= tradeLocations.Count; i++)
        {
            var x = i + 1;
            if (tradeLocations.Count != i)
            {
                AddLocationButton(buttonGroupTran, locationButton.position, x, scale);
            }
            else
            {
                buttonGroupTran.GetChild(0).GetChild(0).GetComponent<Button>().onClick.AddListener( () => { SelectButton(SelectedCateg.Colony, x-1); });
                buttonGroupTran.GetChild(0).SetAsLastSibling();
            }
        }
        
        // Outpost buttons
        buttonGroupTran = transform.GetChild(0).GetChild(2);
        for(int i = 0; i <= outposts.Count; i++)
        {
            var x = i;
            Button button = AddOutpostButton(buttonGroupTran, x);
            if(i == expeditions.Count)
            {
                button.GetComponent<Image>().color = availableColor;
            }
        }
        SceneRefs.tick.timeController.weekEnd += OutpostProduction;

        TMP_Dropdown dropdown = outpostInfo.unconstructed.GetChild(0).GetComponent<TMP_Dropdown>();
        dropdown.options = new List<TMP_Dropdown.OptionData>() { new("Select") };
        foreach (string s in Enum.GetNames(typeof(ResourceType)))
        {
            dropdown.options.Add(new(s));
        }
        if (outposts.Count(q => !q.constructed) > 0)
            SceneRefs.tick.tickAction += outpostInfo.UpdateOutpostProgress;
    }
    
    /// <summary>
    /// Removes and marks states on colony info.
    /// </summary>
    /// <param name="production">The list of Productions that it should mark.</param>
    /// <param name="part">Index of child object.</param>
    void PrepBaseLocation(List<PassiveProduction> production, int part)
    {
        for (int i = 0; i < production.Count; i++)
        {
            Transform cat = colonyTran.GetChild(part).GetChild(i + 1).GetChild(1);
            for (int j = 4; j > -1; j--)
            {
                if (j >= production[i].maxProduction)
                    Destroy(cat.GetChild(j).gameObject);
                else if (j >= production[i].currentProduction)
                    cat.GetChild(j).GetComponent<Image>().color = unavailableColor;
                else
                    cat.GetChild(j).GetComponent<Image>().color = productionColor;
            }
        }
    }
    
    /// <summary>
    /// Creates empty sliders, and disables them.
    /// </summary>
    void AddExpeditionSlider()
    {
        Transform routes = window.transform
            .GetChild(0).GetChild(1);
        GameObject newRoute = Instantiate(expeditionSlider, routes);
        newRoute.SetActive(false);
    }

    /// <summary>
    /// Adds a trade location and conects it with colony.
    /// </summary>
    /// <param name="buttonGroupTran">Parent transform to the button.</param>
    /// <param name="colonyPos">Position of the button.</param>
    /// <param name="x">Location index.</param>
    /// <param name="scale">Canvas scale.</param>
    void AddLocationButton(Transform buttonGroupTran, Vector3 colonyPos, int x, float scale)
    {
        // creates the button
        RectTransform locationButton = Instantiate(tradeButton, buttonGroupTran).GetComponent<RectTransform>();
        locationButton.anchoredPosition = new(tradeLocations[x-1].pos.x, tradeLocations[x-1].pos.z);
        // creates the path between trader and colony
        Vector3 dif = locationButton.position - colonyPos;
        RectTransform routePath = Instantiate(routeImage,
            (locationButton.position + colonyPos) / 2,
            Quaternion.Euler(new Vector3(0, 0, 180 * Mathf.Atan(dif.y / dif.x) / Mathf.PI)),
            window.transform.GetChild(0).GetChild(0)).GetComponent<RectTransform>();

        routePath.sizeDelta = new(Vector3.Distance(locationButton.position, colonyPos) / scale, 2);
        routePath.GetComponent<Image>().color = unavailableColor;
        buttonGroupTran.GetChild(x).GetChild(0).GetComponent<Button>().onClick.AddListener(() => { SelectButton(SelectedCateg.Trade, x - 1); });
    }
    
    /// <summary>
    /// Creates a outpost button.
    /// </summary>
    /// <param name="t">Parent transform to the button.</param>
    /// <param name="x">Outpost index.</param>
    /// <returns></returns>
    public Button AddOutpostButton(Transform t, int x)
    {
        Button button = Instantiate(tradeButton, t).transform.GetChild(0).GetComponent<Button>();
        button.onClick.AddListener(() => { SelectButton(SelectedCateg.Outpost, x); });
        return button;
    }

    /// <summary>
    /// Handles outpost production
    /// </summary>
    void OutpostProduction()
    {
        Resource r = new(-1);
        int money = 0;
        foreach(Outpost o in outposts.Where(q=> q.constructed))
        {
            float mod = 1;
            if (o.timeToFinish > 0)
            {
                mod = (1440 - o.timeToFinish/7) / 1440f;
                o.timeToFinish = 0;
            }
            MyRes.ManageRes(r, o.production, mod);
            money += Outpost.resourceCosts[o.production.type[0]] * Mathf.FloorToInt((Outpost.resourceAmmount[o.production.type[0]] - o.production.ammount[0]) * mod);
        }
        MyRes.DeliverToElevator(r);
        MyRes.ManageMoney(money);
    }

    /// <summary>
    /// Called by buttons, calles button handlers.
    /// </summary>
    /// <param name="selectedCateg">Button category.</param>
    /// <param name="index">Child index.</param>
    public void SelectButton(SelectedCateg selectedCateg, int index)
    {
        EndRename();
        if (selectedCateg == isLastLocation && index == lastIndex)
            return;

        int buttonGroup;
        if (ButtonChange(false, out buttonGroup))
            window.transform.GetChild(buttonGroup).GetChild(lastIndex).GetChild(0).GetComponent<Animator>().SetTrigger("unselected");

        isLastLocation = selectedCateg;
        lastIndex = index;
        if (ButtonChange(true, out buttonGroup))
            window.transform.GetChild(buttonGroup).GetChild(lastIndex).GetChild(0).GetComponent<Animator>().SetTrigger("selected");
    }

    /// <summary>
    /// Handles deselecting/selecting buttons.
    /// </summary>
    /// <param name="activate">If it should be selected or not.</param>
    /// <param name="buttonGroup">The output button group.</param>
    /// <returns>If the values be used.</returns>
    bool ButtonChange(bool activate, out int buttonGroup)
    {
        buttonGroup = -1;
        switch (isLastLocation)
        {
            //
            case SelectedCateg.Colony:
                colonyInfo.gameObject.SetActive(activate);
                buttonGroup = 1;

                if (activate)
                {
                    header.text = colonyLocation.name;
                }
                break;
            //
            case SelectedCateg.Trade:
                tradeInfo.gameObject.SetActive(activate);
                buttonGroup = 1;
                if (activate)
                {
                    header.text = tradeInfo.ChangeTradeLocation(tradeLocations[lastIndex]);
                    window.transform.GetChild(0).GetChild(0).GetChild(lastIndex).GetComponent<Image>().color = availableColor;
                }
                else
                {
                    window.transform.GetChild(0).GetChild(0).GetChild(lastIndex).GetComponent<Image>().color = unavailableColor;
                }
                break;
            //
            case SelectedCateg.Outpost:
                outpostInfo.gameObject.SetActive(activate);
                buttonGroup = 2;

                if (activate)
                {
                    if(lastIndex < outposts.Count) 
                    {
                        header.text = outpostInfo.ChangeOutpost(lastIndex);
                    }
                    else
                    {
                        header.text = outpostInfo.NewOutpostView(lastIndex);
                    }
                }
                break;
            // Do not do anything
            case SelectedCateg.None:
            default:
                return false;
        }
        return true;
    }
    

    public void StartExpediton(TradeExpedition expedition)
    {
        Transform t = window.transform.GetChild(0).GetChild(1);
        for(int i = 0; i < t.childCount; i++)
            if (!t.GetChild(i).gameObject.activeSelf)
            {
                expedition.sliderID = i;
                break;
            }

        Slider slider = t.GetChild(expedition.sliderID).GetComponent<Slider>();
        slider.value = 0;
        slider.handleRect.GetComponent<ExpeditionInfo>().SetExpedition(expedition);
        slider.gameObject.SetActive(true);

        Transform tradeLocationsTran = transform.GetChild(0).GetChild(1);
        Vector3 colonyTran = tradeLocationsTran.GetChild(tradeLocationsTran.childCount - 1).transform.position;
        Vector3 posTran = tradeLocationsTran.GetChild(lastIndex).position;
        Vector3 dif = posTran - colonyTran;

        slider.transform.position = (posTran + colonyTran) / 2;
        slider.maxValue = (Vector3.Distance(posTran, colonyTran) - 15);
        slider.GetComponent<RectTransform>().sizeDelta = new((slider.maxValue - 15) / gameObject.GetComponent<Canvas>().scaleFactor, slider.GetComponent<RectTransform>().sizeDelta.y);

        float f = Mathf.Atan(dif.y / dif.x);
        slider.transform.rotation = Quaternion.Euler(new Vector3(0, 0, dif.x > 0 ? (180 * f / Mathf.PI) : -180 + (180 * f / Mathf.PI)));

        expedition.tradeLocation = lastIndex;
        expedition.maxProgress = slider.maxValue;
        expeditions.Add(expedition);
        if (expeditions.Count == 1)
        {
            SceneRefs.tick.tickAction += MoveTradeRouteProgress;
            StartCoroutine(MoveTradeRoute());
        }
    }

    void LoadExpedition(TradeExpedition expedition)
    {
        Transform t = transform.GetChild(0).GetChild(0).GetChild(1);
        Transform tradeLocationsTran = transform.GetChild(0).GetChild(1);

        Vector3 colonyTran = tradeLocationsTran.GetChild(tradeLocationsTran.childCount - 1).transform.position;
        Vector3 posTran = tradeLocationsTran.GetChild(expedition.tradeLocation).position;
        Vector3 dif = posTran - colonyTran;

        float tangens = Mathf.Atan(dif.y / dif.x);
        Slider slider = t.GetChild(expedition.sliderID).GetComponent<Slider>();
        slider.maxValue = expedition.maxProgress;

        slider.GetComponent<RectTransform>().sizeDelta = 
            new(
                (slider.maxValue - 15) / gameObject.GetComponent<Canvas>().scaleFactor, 
                slider.GetComponent<RectTransform>().sizeDelta.y);

        slider.transform.rotation = 
            Quaternion.Euler(new Vector3(0, 0, 
                dif.x > 0 
                    ? (180 * tangens / Mathf.PI) 
                    : -180 + (180 * tangens / Mathf.PI)));
        slider.transform.position = (posTran + colonyTran) / 2;
        slider.handleRect.GetComponent<ExpeditionInfo>().SetExpedition(expedition);
        slider.gameObject.SetActive(true);
        slider.value = expedition.currentProgress;
    }

    void MoveTradeRouteProgress()
    {
        foreach(TradeExpedition exp in expeditions)
        {
            exp.currentProgress += exp.goingToTrade ? 4 : -4;
        }
    }

    IEnumerator MoveTradeRoute()
    {
        while (true)
        {
            for(int i = expeditions.Count -1; i >= 0; i--)
            {
                TradeExpedition exp = expeditions[i];
                Slider slider = window.transform.GetChild(0).GetChild(1).GetChild(exp.sliderID).GetComponent<Slider>();
                slider.value = Mathf.Lerp(slider.value, exp.currentProgress, expeditionSpeed * Time.deltaTime * Time.timeScale);
                slider.handleRect.GetComponent<ExpeditionInfo>().MoveInfo();
                if(exp.goingToTrade ? slider.value >= slider.maxValue : slider.value <= 0)
                {
                    if (exp.FinishExpedition())
                    {
                        expeditions.RemoveAt(i);
                        tradeInfo.UpdateTradeText();
                        if (expeditions.Count == 0)
                        {
                            SceneRefs.tick.tickAction -= MoveTradeRouteProgress;
                            yield break;
                        }
                    }
                }
            }
            yield return null;
        }
    }

    public void EndRename()
    {
        if(header.text.Trim().Length > 0)
        {
            switch (isLastLocation)
            {
                case SelectedCateg.Colony:
                    colonyLocation.name = header.text;
                    break;
                case SelectedCateg.Trade:
                    tradeLocations[lastIndex].name = header.text;
                    break;
                case SelectedCateg.Outpost:
                    if(lastIndex < outposts.Count)
                        outposts[lastIndex].name = header.text;
                    break;
                case SelectedCateg.None:
                default:
                    break;
            }
        }
        else
        {
            switch (isLastLocation)
            {
                case SelectedCateg.Colony:
                    header.text = colonyLocation.name;
                    break;
                case SelectedCateg.Trade:
                    header.text = tradeLocations[lastIndex].name;
                    break;
                case SelectedCateg.Outpost:
                    if (lastIndex < outposts.Count)
                        header.text = outposts[lastIndex].name;
                    break;
                case SelectedCateg.None:
                default:
                    break;

            }
        }
        EventSystem.current.SetSelectedGameObject(null);
    }
}