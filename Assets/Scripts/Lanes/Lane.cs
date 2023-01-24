using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// TODO: Look at how the waste is being added.
// When the lane is waiting for the truck to come to the skip, it should let stuff pile up?
// Definitely shouldn't show the button!
// Shouldn't allow new trucks to come while waitin for one.

public class Lane : MonoBehaviour
{
    public int laneIndex;

    // The only reason these need to be public is because LaneEditor needs access to them to draw the handles.
    [Tooltip("Where the trucks go to take the waste out of the skip.")]
    public Vector3 truckSkipPosition;
    [Tooltip("Where the workers go to place the waste to put in the skip.")]
    public Vector3 workerSkipPosition;
    public Transform managerSpawn;

    public Vector3 farEndPosition;

    [SerializeField, Tooltip("How much waste to be put in the skip before moving to the next stage of filled.")]
    private int amountOfWasteBeforeFilling;
    [SerializeField, Tooltip("How much waste to remove when tapping on the skip.")]
    private int amountOfWasteToRemove;

    private readonly int numBlendShapes = 4;
    private int maxWaste;

    [HideInInspector]
    public List<Worker> workerList;

    private Waste[] wasteList;
    [HideInInspector]
    public int totalWaste;

    [SerializeField]
    private Sprite clearWasteImage;
    [SerializeField]
    private Sprite contaminatedWasteImage;

    private bool isMixedWaste;
    public Waste.Type[] acceptedWasteTypes; //for assigning what type of waste should be accepted into that lane without making the skips 'mixed'

    public bool hasManager;
    private bool waitingForTruck;

    private Money money;
    private AudioSource audioSource;

    [SerializeField]
    private GameObject truck;

    private Level level;

    // Like Start, but called explicitly from AllLanes so we can guarantee that the Lane has been properly initialised when the LaneManager needs it to be.
    public void Setup(int index)
    {
        money = GameObject.FindGameObjectWithTag("Money").GetComponent<Money>();
        laneIndex = index;

        truckSkipPosition += transform.position;
        workerSkipPosition += transform.position;
        managerSpawn.position += transform.position;
        farEndPosition += transform.position;

        workerList = new List<Worker>();

        maxWaste = amountOfWasteBeforeFilling * numBlendShapes;
        wasteList = new Waste[maxWaste];

        audioSource = GetComponent<AudioSource>();

        level = GameObject.Find("GameController").GetComponent<Level>();
    }

    public bool AddWaste(Waste waste)
	{
        wasteList[totalWaste] = waste;
        totalWaste += 1;
        CheckForGameOver();

        UpdateClearWasteButton();

        if ((totalWaste % amountOfWasteBeforeFilling) == 0)
        {
            UpdateSkipVisually();
        }

        PossiblyOverflowAlert();
        
        bool wasMixedWaste = isMixedWaste;
        // Don't call DetermineSkipStatus here because the only thing that's changing is the one waste that we've added.
        if (System.Array.TrueForAll(acceptedWasteTypes, type => waste.type != type))
        {
            isMixedWaste = true;
        }
        
        return (!wasMixedWaste && isMixedWaste);
	}

    public void UpdateSkipVisually()
    {
        SkinnedMeshRenderer[] smrs = GetComponentsInChildren<SkinnedMeshRenderer>();
        for (int i = 0; i < numBlendShapes; i += 1)
        {
            if (i == numBlendShapes - (totalWaste / amountOfWasteBeforeFilling))
            {
                smrs[0].SetBlendShapeWeight(i, 100);
                smrs[1].SetBlendShapeWeight(i, 100);
            }
            else
            {
                smrs[0].SetBlendShapeWeight(i, 0);
                smrs[1].SetBlendShapeWeight(i, 0);
            }
        }
    }

    public void AddWorker(Worker worker)
	{
        workerList.Add(worker);
	}

    public void RemoveWorker(Worker worker)
	{
        workerList.Remove(worker);
	}

    public void AddManager(Manager manager)
    {
        manager.lane = this;
        hasManager = true;
        GetComponent<ParticleSystem>().Play();
    }

    private void CheckForGameOver()
    {
        if (totalWaste == maxWaste)
        {
            Debug.Log("Game OVER!\nYou lose! Good day sir!");
            GameObject.FindWithTag("Menus").GetComponent<Menus>().LoseGame(Menus.Cause.Skip);
        }
    }

    public void CallTruck()
    {
        // This shouldn't be possible since the buttons would be hidden but just in case.
        if (waitingForTruck)
            return;

        if (!isMixedWaste && !money.HaveEnoughMoney(Money.Action.ClearSortedWaste))
            return;
        if (isMixedWaste && !money.HaveEnoughMoney(Money.Action.ClearMixedWaste))
            return;

        if (totalWaste > 0)
        {
            bool refundWaste = false;

            //foreman stuff
            WasteGeneration wg = GameObject.FindGameObjectWithTag("WasteGenerator").GetComponent<WasteGeneration>();
            if (wg != null && wg.hasForeman)
            {
                wg.skipClearedCounter++;
                if (wg.skipClearedCounter % 2 == 0)
                {
                    refundWaste = true;
                }
            }

            audioSource.Play(0);
            if (isMixedWaste)
            {
                money.DeductMoney(Money.Action.ClearMixedWaste);
                if (refundWaste)
                    money.AddMoney(Money.Action.RefundMixedWaste);
            }
            else
            {
                money.DeductMoney(Money.Action.ClearSortedWaste);
                if (refundWaste)
                    money.AddMoney(Money.Action.RefundSortedWaste);
            }

			foreach (var button in GetComponentsInChildren<ClearWasteButton>())
			{
				button.Disable();
			}
			transform.Find("TruckIcon").GetComponent<SpriteRenderer>().enabled = true;
			Vector3 position = level.offScreenPoint;
			position.y = truck.transform.position.y;
			Truck t = Instantiate(truck, position, Quaternion.identity).GetComponent<Truck>();
			t.Pickup(this);
        }
    }

    public void DoneWaitingForTruck()
    {
        waitingForTruck = false;
        foreach (var button in GetComponentsInChildren<ClearWasteButton>())
        {
            button.Enable();
        }
        transform.Find("TruckIcon").GetComponent<SpriteRenderer>().enabled = false;
    }

    public void RemoveWaste()
    {
        // Right let's do this and do it right once and for all.
        // Let's do it the easy way.
        // Remove waste from the bottom of the array.
        int removedWaste = 0;
        for (;;)
        {
            if (removedWaste == amountOfWasteToRemove || removedWaste == totalWaste)
            {
                break;
            }
            Destroy(wasteList[removedWaste].gameObject);
            removedWaste += 1;
        }

        if (removedWaste != totalWaste)
        {
            // Move the waste at the top of the wasteList to the bottom of a new wasteList
            Waste[] newWasteList = new Waste[maxWaste];
            int newWasteIndex = 0;
            for (int oldWasteIndex = removedWaste; oldWasteIndex < totalWaste; oldWasteIndex += 1)
            {
                newWasteList[newWasteIndex++] = wasteList[oldWasteIndex];
            }
            wasteList = newWasteList;
        }
        totalWaste -= removedWaste;

        DetermineSkipStatus();
        UpdateClearWasteButton();
        UpdateSkipVisually();
        PossiblyOverflowAlert();
    }

    public void DetermineSkipStatus()
    {
        isMixedWaste = false;

        for (int i = 0; i < totalWaste; i++)
        {
            if (System.Array.TrueForAll(acceptedWasteTypes, type => wasteList[i].type != type))
            {
                isMixedWaste = true;

                break;
            }
        }
    }

    private void UpdateClearWasteButton()
    {
        if (waitingForTruck)
            return;

        ClearWasteButton[] buttons = GetComponentsInChildren<ClearWasteButton>();

        if (totalWaste == 0)
        {
            foreach (ClearWasteButton button in buttons)
            {
                button.Disable();
            }
        }
        else
        {
            foreach (ClearWasteButton button in buttons)
            {
                button.UpdateSprite(clearWasteImage, contaminatedWasteImage, isMixedWaste);
                button.Enable();
            }
        }
    }

    private void PossiblyOverflowAlert()
    {
        Alerts alerts = GameObject.FindGameObjectWithTag("Alerts").GetComponent<Alerts>();
        if (totalWaste >= maxWaste / 2)
        {
            alerts.ShowLaneAlert(this);
        }
        else
        {
            alerts.HideAlert(this);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Vector3 add = Vector3.zero;
        if (!Application.isPlaying)
		{
            add = transform.position;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(managerSpawn.position + add, 0.6f);

        Gizmos.color = new Color(0.4f, 0.32f, 0.75f);
        Gizmos.DrawSphere(truckSkipPosition + add, 0.6f);

        Gizmos.color = new Color(0.4f, 0.75f, 0.32f);
        Gizmos.DrawSphere(workerSkipPosition + add, 0.6f);

        Gizmos.color = new Color(1.0f, 0.8f, 0.3f);
        Gizmos.DrawSphere(farEndPosition + add, 0.6f);
    }
#endif
}
