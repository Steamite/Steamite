using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OutpostInfo : MonoBehaviour
{
    [Header("Local References")]
    public Transform unconstructed;
    public Transform constructed;
    public Transform inConstruction;

    [Header("Colors")]
    [SerializeField] Color sellColor;
    [SerializeField] Color keepColor;
    [SerializeField] Color balanceColor;

    [Header("")]
    [SerializeField] Trade trade;

    int activeOutpost = 0;

    /// <summary>
    /// Creates the page for a outpost.
    /// </summary>
    /// <param name="outpostIndex"></param>
    /// <returns></returns>
    public string ChangeOutpost(int outpostIndex)
    {
        unconstructed.gameObject.SetActive(false);
        activeOutpost = outpostIndex;
        Outpost outpost = trade.outposts[activeOutpost];
        if (outpost.constructed == false)
        {
            inConstruction.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = $"Level {outpost.level}";
            inConstruction.GetChild(0).GetChild(2).GetComponent<TMP_Text>().text = $"Level {outpost.level+1}";
            inConstruction.GetChild(2).GetComponent<TMP_Text>().text = $"{outpost.timeToFinish/4} hours  {outpost.timeToFinish % 4 * 15} minutes";
            constructed.gameObject.SetActive(false);
            inConstruction.gameObject.SetActive(true);
        }
        else
        {
            constructed.GetChild(0).GetComponent<TMP_Text>().text = $"Level: {outpost.level}";
            Slider slider = constructed.GetChild(2).GetComponent<Slider>();
            slider.value = outpost.production.ammount[0];
            slider.maxValue = outpost.level * Outpost.resourceAmmount[outpost.production.type[0]];

            inConstruction.gameObject.SetActive(false);
            constructed.gameObject.SetActive(true);
            UpdateButton(constructed.GetChild(5));
        }
        return outpost.name;
    }

    /// <summary>
    /// Creates the page for a new outpost.
    /// </summary>
    /// <param name="outpostCount"></param>
    /// <returns></returns>
    public string NewOutpostView(int outpostCount)
    {
        activeOutpost = outpostCount;
        constructed.gameObject.SetActive(false);
        inConstruction.gameObject.SetActive(false);
        unconstructed.gameObject.SetActive(true);
        
        unconstructed.GetChild(0).GetComponent<TMP_Dropdown>().value = 0;
        UpdateButton(unconstructed.GetChild(2));
        return "Outpost " + outpostCount;
    }

    /// <summary>
    /// Checks if the upgradeButton should be interactible or not.
    /// </summary>
    /// <param name="confirmTran"></param>
    public void UpdateButton(Transform confirmTran)
    {
        string s;
        bool useButton = true;
        int outpostLevel = 0;
        if (activeOutpost == trade.outposts.Count)
        {
            if (confirmTran.parent.GetChild(0).GetComponent<TMP_Dropdown>().value == 0)
                useButton = false;
        }
        else
        {
            outpostLevel = trade.outposts[activeOutpost].level;
        }

        int neededMoney = Outpost.upgradeCosts[outpostLevel].money;
        confirmTran.GetChild(0).GetComponent<TMP_Text>().text =
            $"{Outpost.upgradeCosts[outpostLevel].timeInTicks/4f} Hours";

        
        if (neededMoney > MyRes.money)
        {
            useButton = false;
            s = $"<color=red>Money: {MyRes.money}/{neededMoney}</color>\n";
        }
        else
        {
            s = $"Money: {MyRes.money}/{neededMoney}\n";
        }

        if (Outpost.upgradeCosts[outpostLevel].resource.ToStringTMP(ref s, MyRes.resources))
            useButton = false;
        confirmTran.GetChild(1).GetComponent<Button>().interactable = useButton;
        confirmTran.GetChild(2).GetComponent<TMP_Text>().text = s;
    }

    
    /// <summary>
    /// Triggered by changing the slider on Constucted page.
    /// </summary>
    public void SliderChange()
    {
        Slider slider = constructed.GetChild(2).GetComponent<Slider>();
        float b = slider.maxValue - slider.value;
        if (slider.value > b)
            slider.handleRect.GetComponent<Image>().color = sellColor;
        else if (slider.value == b)
            slider.handleRect.GetComponent<Image>().color = balanceColor;
        else
            slider.handleRect.GetComponent<Image>().color = keepColor;

        // Update text
        Outpost outpost = trade.outposts[activeOutpost];
        outpost.production.ammount[0] = (int)slider.value;
        int money = Outpost.resourceCosts[outpost.production.type[0]] * ((Outpost.resourceAmmount[outpost.production.type[0]] * outpost.level) - outpost.production.ammount[0]);
        constructed.GetChild(3).GetChild(1).GetComponent<TMP_Text>().text =
            $"Money: {money}" +
            $"\n{outpost.production.ToStringComplete()}";
    }

    /// <summary>
    /// Triggered by the upgrade button on Constructed and Unconstructed page.
    /// </summary>
    public void OutpostUpgrade()
    {
        if(activeOutpost == trade.outposts.Count)
        {
            trade.outposts.Add(
                new(
                    "Outpost " + activeOutpost, 
                    (ResourceType)unconstructed.GetChild(0).GetComponent<TMP_Dropdown>().value-1));
        }
        else
        {
            trade.outposts[activeOutpost].StartUpgrade();
        }
        if (trade.outposts.Count(q => !q.constructed) == 1)
            MyGrid.sceneReferences.GetComponent<Tick>().tickAction += UpdateOutpostProgress;
        ChangeOutpost(activeOutpost);
    }

    public void UpdateOutpostProgress()
    {
        for(int i = 0; i < trade.outposts.Count; i++)
        {
            Outpost outpost = trade.outposts[i];
            if (!outpost.constructed)
            {
                outpost.timeToFinish -= 1;
                if (outpost.timeToFinish == 0)
                {
                    outpost.Upgrade();
                    if (activeOutpost == i)
                        ChangeOutpost(i);
                    if (trade.outposts.Count(q => !q.constructed) == 0)
                    {
                        MyGrid.sceneReferences.GetComponent<Tick>().tickAction -= UpdateOutpostProgress;
                    }
                }
                else
                {
                    if (activeOutpost == i)
                        inConstruction.GetChild(2).GetComponent<TMP_Text>().text = $"{outpost.timeToFinish / 4} hours  {outpost.timeToFinish % 4 * 15} minutes";
                }
            }
        }
    }
}