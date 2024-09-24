using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    [SerializeField] Transform categoriesTran;
    private ResearchBackend Backend;
    public TMP_Text counter;

    //RICHARD
    [Header("Build Menu")]
    [SerializeField] GameObject buttonGroupPref;
    [SerializeField] BuildButton buttonBuildPref;

    [Header("Research")]
    [SerializeField] ResearchData researchData;
    [SerializeField] Button simpleButtonPref;
    [SerializeField] ResearchUIButton researchButtonPref;
    [SerializeField] GameObject researchCategoryPref;
    [SerializeField] Image line;
    //[SerializeField] GameObject buttonGroupPref;
    //[SerializeField] BuildButton buttonBuildPref;
    //Methods
    //Initializes the UI
    public void Initialise()
    {
        gameObject.SetActive(true);
        InitializeBuildButtons();
        InitializeResearchButtons();
        gameObject.SetActive(false);
    }

    //Start Build Buttons
    public void InitializeBuildButtons() //Sorry, ale jinak to nejde kvuli tomu ze komponenty nejsou jeste aktivni :(
    {
        Transform buildMenuCategs = MyGrid.sceneReferences.canvasManager.buildMenu.GetChild(0);
        Transform buildMenuButtons = MyGrid.sceneReferences.canvasManager.buildMenu.GetChild(1);
        for (int i = 0; i < researchData.buildButtons.buildingCategories.Count; i++)
        {
            //Data
            BuildCategWrapper buildCateg = researchData.buildButtons.buildingCategories[i];
            
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

    public void InitializeResearchButtons()
    {
        Vector2 screenScale = new(((float)Screen.width / 1920f), (float)Screen.height / 1080f);
        for (int i = 0; i < researchData.categories.Count; i++)
        {
            ResearchCategory researchCategory = researchData.categories[i];
            Button _switchbutton = Instantiate(simpleButtonPref, categorySwitchesTran);
            var _i = i;
            InitSimpleButton(_switchbutton, researchCategory.categName, () => ChangeCategory(_i));
            RectTransform categ = Instantiate(researchCategoryPref, categoriesTran).GetComponent<RectTransform>();
            categ.gameObject.SetActive(i == 0);
            foreach (ResearchNode node in researchCategory.nodes)
            {
                if (node.buildButton > -1)
                {
                    ResearchUIButton researchButton = Instantiate(researchButtonPref, categ.GetChild((int)node.gp.level + 1).transform);
                    researchButton.Initialize(researchData.buildButtons.buildingCategories[node.buttonCategory].buildings[node.buildButton].name, node, researchCategory.nodes);
                    RectTransform rect = researchButton.GetComponent<RectTransform>();
                    rect.anchoredPosition = new(node.realX * screenScale.x - Screen.width / 2, 0);
                    bool hasLines = false;
                    foreach (int index in node.unlockedBy)
                    {
                        int ap = (int)researchCategory.nodes.FindIndex(q => q.id == index);
                        int level = (int)researchCategory.nodes[ap].gp.level;
                        int x = ap;
                        if(ap > 0)
                            while (researchCategory.nodes[x-1].gp.level == level)
                                x--;
                        CreateLine(researchButton, categ.GetChild(level+1).GetChild(ap-x).GetComponent<ResearchUIButton>(), categ, screenScale.x, hasLines);
                        hasLines = true;
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

    void CreateLine(ResearchUIButton button, ResearchUIButton unlockedByButton, RectTransform categ, float scale, bool exists)
    {
        RectTransform lineTransform;
        // vertical line
        if (unlockedByButton.unlocksLines.Count == 0)
        {
            lineTransform = InitLine(categ, false, scale);
            lineTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ((button.node.gp.level - unlockedByButton.node.gp.level) * 200 - 150) * scale);
            lineTransform.anchoredPosition = new(unlockedByButton.node.realX * scale - Screen.width / 2, -(((button.node.gp.level + unlockedByButton.node.gp.level) * 200 + 150) / 2 * scale) + (Screen.height * 0.4625f));
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
            lineTransform.anchoredPosition = new(((button.node.realX + unlockedByButton.node.realX) / 2 * scale) - Screen.width / 2, -((button.node.gp.level * 200) * scale) + (Screen.height * 0.4625f));
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
    //pop-up after research is finished
    public void ResearchFinishedPopUP()
    {
        //TODO: Add pop-up
    }
    //Changes the category
    public void ChangeCategory(int id)
    {
        selected_category = id;
        for (int i = 0; i < categoriesTran.childCount; i++)
        {
            categoriesTran.GetChild(i).gameObject.SetActive(i == id);
        }
    }

    //Research button click - starts research
    public void OnResearchButtonClick(int id, Button button)
    {
        Backend.StartResearch(id, button);
    }
    
    //Updates the progress counter
    public void UpdateCounter(ResearchStructs research, bool completed = false)
    {
        if (completed)
        {
            counter.text = "Research Progress: Completed";
            return;
        }
        counter.text = ("Research Progress: " + ((int)((float)(research.GetResearchProgress()/research.GetResearchNeeded()))* 100) + "%");
    }
    
    //Opens the research UI
    public void OpenResearchUI()
    {
        gameObject.SetActive(true);
    }
    public void ToogleResearchUI()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
    
    //Closes the research UI
    public void CloseButton()
    {
       gameObject.SetActive(false);
        
    }
}