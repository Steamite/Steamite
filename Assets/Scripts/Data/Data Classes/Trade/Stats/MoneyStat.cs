using TradeData.Stats;
using UnityEngine;

[CreateAssetMenu(fileName = "Passive Income", menuName = "UI Data/Trade/Stats/Passive Income", order = 0)]
public class MoneyStat : ColonyStat
{
	[Header("Bonus")]
	[SerializeField] int moneyPerLevel = 100;


	public override void DoStat()
	{
		MyRes.UpdateMoney(moneyPerLevel * CurrentState);
	}

	public override string GetText(int state)
	{
		return $"Gives {moneyPerLevel*state} money.";
	}

	public override string GetText(bool complete)
	{
		if (complete)
			return "Affects passive income.";
		else
			return $"+ {CurrentState * moneyPerLevel}";
	}
}
