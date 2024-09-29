using Newtonsoft.Json;
using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ResearchUI : MonoBehaviour
{
    //Variables
    private int selected_category;

    [Header("References")]
    [SerializeField] Transform categorySwitchesTran;
    [SerializeField] public Transform categoriesTran;
    private ResearchBackend backend;
    public TMP_Text counter;

    [Header("Build Menu")]
    [SerializeField] GameObject buttonGroupPref;
    [SerializeField] BuildButton buttonBuildPref;

    [Header("Research")]
    [SerializeField] Button simpleButtonPref;
    [SerializeField] ResearchUIButton researchButtonPref;
    [SerializeField] GameObject researchCategoryPref;
    [SerializeField] Image line;

    Vector2 screenScale;
    ResearchUIButton selectedButton;
    //-----------------------------------\\
    //-----------Initialization----------\\
    //-----------------------------------\\
    public void NewGame()
    {
        ResearchData data = (ResearchData)Resources.Load("Holders/Data/ResearchData");
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
        gameObject.SetActive(true);
        backend = this.GetComponent<ResearchBackend>();
        backend.Init(this);
        BuildButtonHolder buildButtons = (BuildButtonHolder)Resources.Load("Holders/Data/BuildButtonData");
        InitializeBuildButtons(buildButtons);
        InitializeResearchButtons(buildButtons, researches, _currentResearch);
        gameObject.SetActive(false);

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
        Transform buildMenuCategs = MyGrid.canvasManager.buildMenu.GetChild(0);
        Transform buildMenuButtons = MyGrid.canvasManager.buildMenu.GetChild(1);
        
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
            for(int j = 0; j < buildCateg.buildings.Count; j++)
            {
                BuildButton button = Instantiate(buttonBuildPref, group.transform);
                button.Initialize(buildCateg.buildings[j]);
            }
            categButton.gameObject.SetActive(true);
        }
    }

    void InitializeResearchButtons(BuildButtonHolder buildButtons, ResearchCategory[] researches, int _currentResearch)
    {
        screenScale = new(((float)Screen.width / 1920f), (float)Screen.height / 1080f);
        Vector2 categWindowSize = new(1920 * screenScale.x, 1000 * screenScale.y);
        Transform buildCategTransform = MyGrid.canvasManager.buildMenu.GetChild(1);
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
                    ResearchUIButton researchButton = Instantiate(researchButtonPref, categ.GetChild((int)node.gp.level + 1).transform);
                    researchButton.Initialize( 
                        node,
                        researchCategory.nodes);
                    RectTransform rect = researchButton.GetComponent<RectTransform>();
                    rect.anchoredPosition = new((node.realX - (categWindowSize.x / 2)) * screenScale.x, 0);
                    bool hasLines = false;
                    foreach (int index in node.unlockedBy)
                    {
                        CreateLine(researchButton, GetResearchUIButton(i, index), categ, screenScale.x, categWindowSize, hasLines);
                        hasLines = true;
                    }

                    if(node.id == _currentResearch)
                    {
                        backend.StartResearch(researchButton);
                    }
                }
            }
            foreach(ResearchUIButton researchUIButton in categ.GetComponentsInChildren<ResearchUIButton>())
            {
                researchUIButton.RecolorLines();
            }
        }
    }

    void InitSimpleButton(Button b, string _name, UnityAction action)
    {
        b.name = _name;
        b.transform.GetChild(0).GetComponent<TMP_Text>().text = _name;
        b.onClick.AddListener(action);
    }

    void CreateLine(ResearchUIButton button, ResearchUIButton unlockedByButton, RectTransform categ, float scale, Vector2 categSize, bool exists)
    {
        RectTransform lineTransform;
        // vertical line
        if (unlockedByButton.unlocksLines.Count == 0)
        {
            lineTransform = InitLine(categ, false, scale);
            lineTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ((button.node.gp.level - unlockedByButton.node.gp.level) * 200 - 150) * scale);
            lineTransform.anchoredPosition = new((unlockedByButton.node.realX - categSize.x / 2) * scale, -(((button.node.gp.level + unlockedByButton.node.gp.level) * 200 + 150) / 2 * scale) + (Screen.height * 0.4625f));
            unlockedByButton.unlocksLines.Add(lineTransform.GetComponent<Image>());
        }
        
        // small line
        if (!exists)
        {
            lineTransform = InitLine(categ, false, scale);
            lineTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 40 * scale);
            lineTransform.anchoredPosition = new(button.GetComponent<RectTransform>().anchoredPosition.x, -((button.node.gp.level * 200 + 20)*scale) + (Screen.height * 0.4625f));
            button.unlockedByLines.Add(lineTransform.GetComponent<Image>());
        }
        unlockedByButton.unlocksLines.Add(button.unlockedByLines[0]);

        // horizontal line
        if (Mathf.RoundToInt(unlockedByButton.node.realX) != Mathf.RoundToInt(button.node.realX))
        {
            lineTransform = InitLine(categ, true, scale);
            lineTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (Mathf.Abs(button.node.realX - unlockedByButton.node.realX) + 5) * scale);
            lineTransform.anchoredPosition = new((button.node.realX + unlockedByButton.node.realX - categSize.x)/ 2 * scale, -((button.node.gp.level * 200) * scale) + (Screen.height * 0.4625f));
            button.unlockedByLines.Add(lineTransform.GetComponent<Image>());
            unlockedByButton.unlocksLines.Add(button.unlockedByLines[^1]);
        }

        button.unlockedByLines.Add(unlockedByButton.unlocksLines[0]);
    }

    RectTransform InitLine(RectTransform categ, bool isHorizontal, float scale)
    {
        RectTransform lineTransform = Instantiate(line, categ.GetChild(0)).GetComponent<RectTransform>();
        lineTransform.SetSizeWithCurrentAnchors(isHorizontal ? RectTransform.Axis.Vertical :RectTransform.Axis.Horizontal, 5 * scale);
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

    //Research button click - starts research
    public void ResearchButtonClick(ResearchUIButton button)
    {
        RectTransform rect = MyGrid.canvasManager.infoWindow.transform.GetChild(0).GetComponent<RectTransform>();
        if (button.GetComponent<RectTransform>().anchoredPosition.x < 0)
            rect.anchoredPosition = new(0, 0);
        else
            rect.anchoredPosition = new(-Screen.width + 400, 0);
        rect.parent.gameObject.SetActive(true);
        if (selectedButton == button)
            selectedButton = null;
        else
        {
            selectedButton = button;
            UpdateInfoWindow(selectedButton);
        }
    }

    public void UpdateInfoWindow(ResearchUIButton button)
    {
        if(selectedButton && selectedButton.node.id == button.node.id)
        {
            // info window
            MyGrid.canvasManager.infoWindow.SwitchMods(InfoMode.Research, button.name);
            Transform researchInfo = MyGrid.canvasManager.infoWindow.researchTransform;
            researchInfo.GetChild(0).GetComponent<TMP_Text>().text = $"Unlocks {button.node.name}.";
            switch (button.state)
            {
                case ResearchUIButton.ButtonState.Available:
                    researchInfo.GetChild(2).GetComponent<Button>().interactable = true;
                    researchInfo.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = "research";
                    researchInfo.GetChild(3).GetComponent<TMP_Text>().text = $"{button.node.researchTime * 5} research points needed";
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
    public void InfoWindowButtonClick()
    {
        if (!selectedButton)
            return;
        if(backend.currentResearch == selectedButton)
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
    public void ResearchFinishedPopUP()
    {
        //TODO: Add pop-up
    }

    //-----------------------------------\\
    //-------Toggling reaserchMenu-------\\
    //-----------------------------------\\

    public void ToogleResearchUI()
    {
        if (gameObject.activeSelf)
        {
            CloseButton();
        }
        else
        {
            OpenResearchUI();
        }
    }

    //Opens the research UI
    public void OpenResearchUI()
    {
        // open view Window
        MyGrid.canvasManager.infoWindow.gameObject.SetActive(false);
        gameObject.SetActive(true);
        if (backend.currentResearch)
        {
            StartCoroutine(backend.UpdateButtonFill());
            backend.currentResearch.StartAnim();
        }
    }

    //Closes the research UI
    public void CloseButton()
    {
        if (backend.currentResearch)
            backend.currentResearch.EndAnim();
        gameObject.SetActive(false);
        MyGrid.canvasManager.infoWindow.gameObject.SetActive(false);
    }
}