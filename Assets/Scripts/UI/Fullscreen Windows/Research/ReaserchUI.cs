using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ResearchUI : FullscreenWindow
{
    //Variables
    private int selected_category;

    [Header("References")]
    [SerializeField] Transform categorySwitchesTran;
    [SerializeField] public Transform categoriesTran;
    public ResearchBackend backend;

    [Header("Research open")]
    [SerializeField] public Image openResearchFill;
    [SerializeField] public TMP_Text openResearchText;
    [SerializeField] public Animator openResearchAnimator;

    [Header("Build Menu")]
    [SerializeField] GameObject buttonGroupPref;
    [SerializeField] BuildButton buttonBuildPref;

    [Header("Research")]
    [SerializeField] Button simpleButtonPref;
    [SerializeField] ResearchUIButton researchButtonPref;
    [SerializeField] GameObject researchCategoryPref;
    [SerializeField] Image line;

    [Header("Fill animation")]
    public float speed = 3;
    public float elapsedProgress;
    //Vector2 screenScale;
    ResearchUIButton selectedButton;
    //-----------------------------------\\
    //-----------Initialization----------\\
    //-----------------------------------\\
    public void NewGame()
    {
        ResearchData data = (ResearchData)Resources.Load("Holders/Data/Research Data");
        ResearchCategory[] researches = new ResearchCategory[data.categories.Count];
        for (int i = 0; i < data.categories.Count; i++)
        {
            researches[i] = new(data.categories[i].categName);
            for (int j = 0; j < data.categories[i].nodes.Count; j++)
            {
                researches[i].nodes.Add(new(data.categories[i].nodes[j]));
            }
        }
        Initialize(researches);
    }

    public void LoadGame(ResearchSave researchSave)
    {
        Initialize(researchSave.categories, researchSave.currentResearch);
    }

    void Initialize(ResearchCategory[] researches, int _currentResearch = -1)
    {
        window.gameObject.SetActive(true);
        backend = gameObject.GetComponent<ResearchBackend>();
        backend.Init(this);

        BuildButtonHolder buildButtons = (BuildButtonHolder)Resources.Load("Holders/Models/BuildButton Data");
        InitializeBuildButtons(buildButtons);
        InitializeResearchButtons(buildButtons, researches, _currentResearch);
        window.gameObject.SetActive(false);

        // freeing useless prefabs
        buttonBuildPref = null;
        buttonGroupPref = null;

        simpleButtonPref = null;
        researchButtonPref = null;
        researchCategoryPref = null;

    }

    //Start Build Buttons
    void InitializeBuildButtons(BuildButtonHolder buildButtons)
    {
        Transform buildMenuCategs = CanvasManager.buildMenu.GetChild(0);
        Transform buildMenuButtons = CanvasManager.buildMenu.GetChild(1);

        for (int i = 0; i < buildButtons.buildingCategories.Count; i++)
        {
            //Data
            BuildCategWrapper buildCateg = buildButtons.buildingCategories[i];

            //Category Button
            Button categButton = Instantiate(simpleButtonPref, buildMenuCategs);
            var _i = i;
            InitSimpleButton(categButton, buildCateg.categName, () => categButton.transform.parent.GetComponent<BuildCategoryButton>().ToggleCategory(_i));

            //Button group
            GameObject group = Instantiate(buttonGroupPref, buildMenuButtons);
            group.name = buildCateg.categName;
            group.SetActive(false);
            for (int j = 0; j < buildCateg.buildings.Count; j++)
            {
                BuildButton button = Instantiate(buttonBuildPref, group.transform);
                button.Initialize(buildCateg.buildings[j]);
            }
            categButton.gameObject.SetActive(true);
        }
    }

    void InitializeResearchButtons(BuildButtonHolder buildButtons, ResearchCategory[] researches, int _currentResearch)
    {
        //screenScale = new(((float)Screen.width / 1920f), (float)Screen.height / 1080f);
        Vector2 categWindowSize = new(1920, 1080);
        Transform buildCategTransform = CanvasManager.buildMenu.GetChild(1);
        for (int i = 0; i < researches.Length; i++)
        {
            ResearchCategory researchCategory = researches[i];
            Button _switchbutton = Instantiate(simpleButtonPref, categorySwitchesTran);
            var _i = i;
            InitSimpleButton(_switchbutton, researchCategory.categName, () => ChangeCategory(_i));
            RectTransform categ = Instantiate(researchCategoryPref, categoriesTran).GetComponent<RectTransform>();
            categ.gameObject.SetActive(i == 0);
            categ.name = researches[i].categName;
            foreach (ResearchNode node in researchCategory.nodes)
            {
                if (node.buildButton > -1)
                {
                    ResearchUIButton researchButton = Instantiate(researchButtonPref, categ.GetChild((int)node.gp.y + 1).transform);
                    researchButton.Initialize(
                        node,
                        researchCategory.nodes);
                    RectTransform rect = researchButton.GetComponent<RectTransform>();
                    rect.anchoredPosition = new((node.realX /* screenScale.x*/) - (categWindowSize.x / 2), 0);
                    bool hasLines = false;
                    foreach (int index in node.unlockedBy)
                    {
                        CreateLine(researchButton, GetResearchUIButton(i, index), categ, categWindowSize, hasLines);
                        hasLines = true;
                    }
                    if (node.id == _currentResearch)
                    {
                        backend.StartResearch(researchButton);
                    }
                }
            }
            foreach (ResearchUIButton researchUIButton in categ.GetComponentsInChildren<ResearchUIButton>())
            {
                researchUIButton.RecolorLines();
            }
        }
        if (!backend.currentResearch)
        {
            openResearchText.text = "None";
        }
    }

    void InitSimpleButton(Button b, string _name, UnityAction action)
    {
        b.name = _name;
        b.transform.GetChild(0).GetComponent<TMP_Text>().text = _name;
        b.onClick.AddListener(action);
    }

    void CreateLine(ResearchUIButton button, ResearchUIButton unlockedByButton, RectTransform categ, Vector2 categSize, bool exists)
    {
        RectTransform lineTransform;
        // vertical line
        if (unlockedByButton.unlocksLines.Count == 0)
        {
            lineTransform = InitLine(categ, false);
            lineTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ((button.node.gp.y - unlockedByButton.node.gp.y) * 200 - 150));
            lineTransform.anchoredPosition = new((unlockedByButton.node.realX) - (categSize.x / 2), -(((button.node.gp.y + unlockedByButton.node.gp.y) * 200 + 150) / 2) + (1080 * 0.4625f));
            unlockedByButton.unlocksLines.Add(lineTransform.GetComponent<Image>());
        }

        // small line
        if (!exists)
        {
            lineTransform = InitLine(categ, false);
            lineTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 40);
            lineTransform.anchoredPosition = new(button.GetComponent<RectTransform>().anchoredPosition.x, -((button.node.gp.y * 200 + 20)) + (1080 * 0.4625f));
            button.unlockedByLines.Add(lineTransform.GetComponent<Image>());
        }
        unlockedByButton.unlocksLines.Add(button.unlockedByLines[0]);

        // horizontal line
        if (Mathf.RoundToInt(unlockedByButton.node.realX) != Mathf.RoundToInt(button.node.realX))
        {
            lineTransform = InitLine(categ, true);
            lineTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (Mathf.Abs(button.node.realX - unlockedByButton.node.realX) + 5));
            lineTransform.anchoredPosition = new((((button.node.realX + unlockedByButton.node.realX)) - categSize.x) / 2, -((button.node.gp.y * 200)) + (1080 * 0.4625f));
            button.unlockedByLines.Add(lineTransform.GetComponent<Image>());
            unlockedByButton.unlocksLines.Add(button.unlockedByLines[^1]);
        }

        button.unlockedByLines.Add(unlockedByButton.unlocksLines[0]);
    }

    RectTransform InitLine(RectTransform categ, bool isHorizontal)
    {
        RectTransform lineTransform = Instantiate(line, categ.GetChild(0)).GetComponent<RectTransform>();
        lineTransform.SetSizeWithCurrentAnchors(isHorizontal ? RectTransform.Axis.Vertical : RectTransform.Axis.Horizontal, 5);
        return lineTransform;
    }

    //-----------------------------------\\
    //----------Button Handling----------\\
    //-----------------------------------\\

    //Changes the category
    void ChangeCategory(int id)
    {
        selected_category = id;
        for (int i = 0; i < categoriesTran.childCount; i++)
        {
            categoriesTran.GetChild(i).gameObject.SetActive(i == id);
        }
    }

    //Research button click - opens info window
    public void ResearchButtonClick(ResearchUIButton button)
    {
        RectTransform rect = CanvasManager.infoWindow.transform.GetChild(0).GetComponent<RectTransform>();
        if (button.GetComponent<RectTransform>().anchoredPosition.x < 0)
            rect.anchoredPosition = new(0, 0);
        else
            rect.anchoredPosition = new(-1519, 0);
        if (selectedButton == button)
        {
            selectedButton = null;
            rect.parent.gameObject.SetActive(false);
        }
        else
        {
            rect.parent.gameObject.SetActive(true);
            selectedButton = button;
            UpdateInfoWindow(selectedButton);
        }
    }

    public void UpdateInfoWindow(ResearchUIButton button)
    {
        if (selectedButton && selectedButton.node.id == button.node.id)
        {
            // info window
            CanvasManager.infoWindow.SwitchMods(InfoMode.Research, button.name);
            Transform researchInfo = CanvasManager.infoWindow.researchTransform;
            researchInfo.GetChild(0).GetComponent<TMP_Text>().text = $"Unlocks <color=#D3D3D3>{button.node.name}</color>.";
            //ShowNeededResources(button.node, true);
            switch (button.state)
            {
                case ResearchUIButton.ButtonState.Available:
                    researchInfo.GetChild(2).GetComponent<Button>().interactable = true;
                    researchInfo.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = "research";
                    if (button.node.currentTime == 0)
                        researchInfo.GetChild(3).GetComponent<TMP_Text>().text = $"{button.node.researchTime * 5} research points needed";
                    else
                        researchInfo.GetChild(3).GetComponent<TMP_Text>().text = $"{Mathf.RoundToInt(elapsedProgress * 5)} / {button.node.researchTime * 5} research points";
                    break;
                case ResearchUIButton.ButtonState.Completed:
                    selectedButton = null;
                    researchInfo.GetChild(2).GetComponent<Button>().interactable = false;
                    researchInfo.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = "researched";
                    researchInfo.GetChild(3).GetComponent<TMP_Text>().text = $"{button.node.researchTime * 5} research points";
                    break;
                default:
                    selectedButton = null;
                    researchInfo.GetChild(2).GetComponent<Button>().interactable = false;
                    researchInfo.GetChild(3).GetComponent<TMP_Text>().text = $"{button.node.researchTime * 5} research points needed";
                    break;
            }
        }
    }

    public void ShowNeededResources(ResearchNode node, bool useGlobalStored)
    {
        Transform researchInfo = CanvasManager.infoWindow.researchTransform;
        TMP_Text types = researchInfo.GetChild(1).GetChild(0).GetComponent<TMP_Text>();
        TMP_Text ammounts = researchInfo.GetChild(1).GetChild(1).GetComponent<TMP_Text>();
        types.text = "";
        ammounts.text = "";
        if (node.reseachCost != null && node.reseachCost.type.Count > 0)
        {
            for (int i = 0; i < node.reseachCost.type.Count; i++)
            {
                if (i > 0)
                {
                    types.text += "\n";
                    ammounts.text += "\n";
                }

                types.text += node.reseachCost.type[i].ToString();
                ammounts.text += $"... " +
                    $"{MyRes.resources.ammount[MyRes.resources.type.IndexOf(node.reseachCost.type[i])]} " +
                    $"/ {node.reseachCost.ammount[i].ToString()}";
            }
        }
    }

    public void InfoWindowButtonClick()
    {
        if (!selectedButton)
            return;
        if (backend.currentResearch == selectedButton)
        {
            // stop research
        }
        else
        {
            backend.StartResearch(selectedButton);
        }
    }

    public ResearchUIButton GetResearchUIButton(int categID, int _id)
    {
        int level = -1;
        Transform categ = categoriesTran.GetChild(categID);
        for (level = 1; level <= 5; level++)
        {
            for (int j = 0; j < categ.GetChild(level).childCount; j++)
            {
                ResearchUIButton result = categ.GetChild(level).GetChild(j).GetComponent<ResearchUIButton>();
                if (result.node.id == _id)
                    return result;

            }
        }
        return null;
    }

    //pop-up after research is finished
    public void ResearchFinish()
    {
        backend.FinishResearch();
        openResearchFill.fillAmount = 1;
        openResearchText.text = "100%";
        StartCoroutine(OpenResearchSmoothEnd());
    }

    public IEnumerator UpdateButtonFill()
    {
        elapsedProgress = backend.currentResearch.node.currentTime;
        while (elapsedProgress < backend.currentResearch.node.researchTime)
        {
            elapsedProgress = Mathf.Lerp(elapsedProgress, backend.currentResearch.node.currentTime, Time.deltaTime * speed);
            float fill = elapsedProgress / backend.currentResearch.node.researchTime;
            if (window.gameObject.activeSelf)
            {
                backend.currentResearch.borderFill.fillAmount = fill;
                CanvasManager.infoWindow.researchTransform.GetChild(3).GetComponent<TMP_Text>().text
                    = $"{Mathf.RoundToInt(elapsedProgress * 5)} / {backend.currentResearch.node.researchTime * 5} research points";
            }
            openResearchFill.fillAmount = fill;
            openResearchText.text = $"{Mathf.RoundToInt(fill * 100)}%";
            yield return null;
        }
        // Research Finished
        ResearchFinish();
    }

    IEnumerator OpenResearchSmoothEnd()
    {
        float f = openResearchAnimator.GetFloat("Speed");
        while (f > 0)
        {
            f -= 0.1f;
            openResearchAnimator.SetFloat("Speed", f);
            yield return new WaitForSecondsRealtime(0.1f);
        }
        openResearchAnimator.SetFloat("Speed", 0);
        openResearchFill.fillAmount = 0;
        openResearchText.text = "None";
    }


    //-----------------------------------\\
    //-------Toggling reaserchMenu-------\\
    //-----------------------------------\\

    public override void OpenWindow()
    {
        base.OpenWindow();
        backend.currentResearch?.StartAnim();
    }

    public override void CloseWindow()
    {
        selectedButton = null;
        backend.currentResearch?.EndAnim();
        base.CloseWindow();
    }

}
