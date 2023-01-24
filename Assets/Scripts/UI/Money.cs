using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Money : MonoBehaviour
{
    public int initialBudget = 3500;
    [HideInInspector]
    public int amountHeld;

    //values for the grading system, changed into hard number barriers instead of percentages like requested
    [SerializeField]
    private int threeStarRequiredAmount = 1750;
    [SerializeField]
    private int twoStarRequiredAmount = 1400;
    [SerializeField]
    private int oneStarRequiredAmount = 1050;

    //values for various actions
    [SerializeField]
    private int sortedWasteSkipClearCost = 75;
    [SerializeField]
    private int mixedWasteSkipClearCost = 450;
    [SerializeField]
    private int hireWorkerCost = 50;
    [SerializeField]
    private int hireManagerCost = 300;
    [SerializeField]
    private int hireForemanCost = 800;

    private Text displayText;

    [SerializeField]
    private GameObject moneyFloatingText;

    public enum Action
    {
        ClearSortedWaste = 0,
        ClearMixedWaste = 1,
        RefundSortedWaste = 2,
        RefundMixedWaste = 3,
        HireWorker = 4,
		HireManager = 5,
        HireForeman = 6
	}
	private Dictionary<Action, int> actionCosts = new Dictionary<Action, int>();
	
    private void Start()
    {
        amountHeld = initialBudget;
        displayText = GetComponentInChildren<Text>();

        UpdateDisplayedAmount();

        actionCosts[Action.ClearSortedWaste] = sortedWasteSkipClearCost;
        actionCosts[Action.ClearMixedWaste] = mixedWasteSkipClearCost;
        actionCosts[Action.RefundSortedWaste] = (int)(sortedWasteSkipClearCost * 0.8);
        actionCosts[Action.RefundMixedWaste] = (int)(mixedWasteSkipClearCost * 0.8);
        actionCosts[Action.HireWorker] = hireWorkerCost;
        actionCosts[Action.HireManager] = hireManagerCost;
        actionCosts[Action.HireForeman] = hireForemanCost;
    }

    private void UpdateDisplayedAmount()
    {
        displayText.text = amountHeld.ToString();
    }

    public void AddMoney(Action action)
    {
        amountHeld += actionCosts[action];
        UpdateDisplayedAmount();

        GameObject floaty = Instantiate(moneyFloatingText, transform.GetChild(0).transform);
        floaty.GetComponent<FloatingMoneyText>().SetString(positiveSign:true, "£" + actionCosts[action].ToString());

        floaty.transform.position = new Vector3(displayText.transform.position.x, displayText.transform.position.y - 150, floaty.transform.position.z);
    }

    public void DeductMoney(Action action)
    {
        amountHeld -= actionCosts[action];
        if (amountHeld < 0) amountHeld = 0;
        UpdateDisplayedAmount();

        GameObject floaty = Instantiate(moneyFloatingText, transform.GetChild(0).transform);
        floaty.GetComponent<FloatingMoneyText>().SetString(positiveSign:false, "£" + actionCosts[action].ToString());

        floaty.transform.position = new Vector3(displayText.transform.position.x + 40, displayText.transform.position.y - 150, floaty.transform.position.z);
    }

    public void DeductMoney(Action action, Touch t)
    {
        amountHeld -= actionCosts[action];
        UpdateDisplayedAmount();

        GameObject floaty = Instantiate(moneyFloatingText, transform.GetChild(0).transform);
        floaty.GetComponent<FloatingMoneyText>().SetString(positiveSign:false, "£" + actionCosts[action].ToString());

        floaty.transform.position = new Vector3(t.position.x, t.position.y, floaty.transform.position.z);
    }

    public bool HaveEnoughMoney(Action action)
    {
        return amountHeld >= actionCosts[action];
    }

    public int CalculatePlayerGrade()
    {
        if (amountHeld >= threeStarRequiredAmount) //if player saved 50% or more of the initial budget they get an A
        {
            return 3;
        }
        else if (amountHeld >= twoStarRequiredAmount)
        {
            return 2;
        }
        else if (amountHeld >= oneStarRequiredAmount)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }
}
