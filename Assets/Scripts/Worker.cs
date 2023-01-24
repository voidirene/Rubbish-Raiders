using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Worker : MonoBehaviour, ISerializationCallbackReceiver
{
    enum WorkerState
    {
        Idling = 0,
        Resting = 1,
        ChangingLane = 2,
        PickingUpWaste = 3,
        MovingToSkip = 4,
        WaitingForAnimation = 5,
    }
    [SerializeField]
    private WorkerState currentState;

    private Movement movement;

    [SerializeField, Tooltip("Effectively, how long does the worker stay out before needing to take a break.")]
    private float maxEnergy;
    private float _energy;
    /// <summary>
    /// <get>Get => returns the current energy</get>
    /// <set>Set => sets the energy to the value, and updates the energy bar</set>
    /// </summary>
    private float Energy
    {
        get
        {
            return _energy;
        }
        set
        {
            _energy = value;
            Vector3 scale = energyBar.transform.localScale;
            scale.x = Mathf.Lerp(0, 1, _energy/maxEnergy);
            energyBar.transform.localScale = scale; //NOTE(irene): this should be changed so that the energy bar doesn't get scaled down, because it looks weird with the new UI
        }
    }
    [SerializeField, Tooltip("How fast does the unit's energy recover while idling.")]
    private float energyRecoveryWhileIdling;
    [SerializeField, Tooltip("How fast does the unit's energy recover while resting.")]
    private float energyRecoveryWhileResting;
    [SerializeField, Tooltip("How fast does the unit's energy drain while working (usually, moving waste).")]
    private float energyDrainWhileWorking;
    
    private Transform canvas;
    private Transform energyBar;
    private Vector3 hqPosition;
    private WasteGeneration wasteGenerator;

    private Waste wasteToPickUp;
    [HideInInspector]
    public Waste heldWaste;

    private AllLanes allLanes;
    public Lane currentLane;

    private Animator anim;

    private bool atHq = false;

    private Image radialTimer;
    [SerializeField]
    private float minigameDuration = 10f;
    private Level level;

    private void Start()
    {
        movement = GetComponent<Movement>();
        anim = GetComponentInChildren<Animator>();
        allLanes = GameObject.Find("GameController").GetComponent<AllLanes>();
        level = GameObject.Find("GameController").GetComponent<Level>();

        hqPosition = transform.Find("/HQPosition").position;
        wasteGenerator = GameObject.Find("WasteGenerator").GetComponent<WasteGeneration>();

        canvas = transform.Find("Canvas");
        energyBar = canvas.Find("EnergyBackground/EnergyBar");
        Energy = _energy;

        //regularShader = Shader.Find("Standard");
        //highlightShader = Shader.Find("TSF/BaseOutline1");

        radialTimer = canvas.Find("RadialTimer/RadialTimerRed/RadialTimerGreen").GetComponent<Image>();

        currentState = WorkerState.Idling;
    }

    private void Update()
    {
        canvas.rotation = Quaternion.identity;
        anim.SetBool("walking", movement.currentVelocity.sqrMagnitude > 0);
        anim.SetFloat("energy", Energy/maxEnergy);

        switch (currentState)
        {
            case WorkerState.Idling:
            {
                // If there's waste spawned in the generator, set it to be the waste to pick up, and change state to be picking it up.
                if ((wasteToPickUp = wasteGenerator.GetFirstFreeWaste(currentLane)) != null)
                {
                    wasteToPickUp.hasBeenTargeted = true;
                    currentState = WorkerState.PickingUpWaste;
                    break;
                }
                else
                {
                    movement.currentVelocity = Vector3.zero;
                    Energy += (energyRecoveryWhileIdling * Time.deltaTime);
                    if (Energy >= maxEnergy) Energy = maxEnergy;
                }
                break;
            }

            case WorkerState.Resting:
            {
                if (atHq)
                {
                    if (DoneResting())
                    {
                        atHq = false;
                        currentState = WorkerState.Idling;
                        break;
                    }
                }
                else
                {
                    if (movement.Pathfind(hqPosition, 3.0f))
                    {
                        atHq = true;
                    }
                }
                break;
            }
            
            case WorkerState.ChangingLane:
            {
                if (movement.Pathfind(currentLane.farEndPosition, 2.0f))
                {
                    currentState = WorkerState.Idling;
                    break;
                }
                break;
            }
            
            case WorkerState.PickingUpWaste:
            {
                if (ShouldRest(energyDrainWhileWorking))
                {
                    UntargetWaste();
                    currentState = WorkerState.Resting;
                    break;
                }
                
                if (movement.Pathfind(wasteToPickUp.transform.position, 0.0f))
                {
                    StartCoroutine(PickUpWaste());
                }
                break;
            }
            
            case WorkerState.MovingToSkip:
            {
                if (ShouldRest(energyDrainWhileWorking))
                {
                    StartCoroutine(DropWaste());
                    break;
                }

                if (movement.Pathfind(currentLane.workerSkipPosition, 2.0f))
                {
                    StartCoroutine(DropWasteAtSkip());
                }
                break;
            }
            
            case WorkerState.WaitingForAnimation:
            {
                break;
            }

            default:
            {
                throw new System.NotImplementedException();
            }
        }
    }

    public void OnBeforeSerialize()
    {
        _energy = maxEnergy;
    }

    public void OnAfterDeserialize()
    {
        _energy = maxEnergy;
    }

    void ShowRadialTimer()
    {
        movement.baseSpeed = 0;

        canvas.Find("RadialTimer").gameObject.SetActive(true);
        StartCoroutine(RadialTimerCountdown(minigameDuration));
    }

    void HideRadialTimer()
    {
        movement.baseSpeed = heldWaste.speedModifier;
        canvas.Find("RadialTimer").gameObject.SetActive(false);
        StopCoroutine(nameof(RadialTimerCountdown));
        GameObject.FindWithTag("GameController").GetComponent<TouchControls>().HideModelView();
    }

    IEnumerator RadialTimerCountdown(float duration)
    {
        float startTime = Time.time;
        float t = duration;
        float fillValue;

        while (Time.time - startTime < duration)
        {
            t -= Time.deltaTime;
            fillValue = t / duration;
            radialTimer.fillAmount = fillValue;
            yield return null;
        }

        if (radialTimer.fillAmount <= 0) //when timer runs out
        {
            movement.baseSpeed = heldWaste.speedModifier;
            canvas.Find("RadialTimer").gameObject.SetActive(false);

            wasteGenerator.timesFailedMinigame++;

            int randomValue = Random.Range(0, allLanes.lanes.Length);
            ChangeLanesTo(randomValue);
            currentState = WorkerState.MovingToSkip;
        }
    }

    /// <summary>
    /// Takes an amount to deplete this unit's energy by. Returns whether the unit should go to rest.
    /// </summary>
    private bool ShouldRest(float energyDrain)
    {
        Energy -= (energyDrain * Time.deltaTime);
        // If the sorter has run out of energy
        if (Energy <= 0.0f)
        {
            // Rest
            Energy = 0.0f;
            HideRadialTimer();
            return true;
        }
        return false;
    }

    /// <summary>
    /// The worker regains energyRecoveryWhileResting amount of energy per frame, until their energy bar is full.
    /// </summary>
    /// <returns>
    /// Whether the unit has finished resting.
    /// </returns>
    private bool DoneResting()
    {
        movement.currentVelocity = Vector3.zero;
        Energy += (energyRecoveryWhileIdling * Time.deltaTime);
        if (Energy >= maxEnergy)
        {
            Energy = maxEnergy;
            return true;
        }
        return false;
    }

    private IEnumerator PlayPutDownAnimation()
    {
        movement.baseSpeed = 1.0f; //reset the speed back to the default
        
        movement.currentVelocity = Vector3.zero;
        anim.Play("Put Down");
        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(info.length);
    }
    
    // Add the held waste to the skip's waste list
    private IEnumerator DropWasteAtSkip()
    {
        currentState = WorkerState.WaitingForAnimation;
        Waste w = heldWaste;

        yield return StartCoroutine(PlayPutDownAnimation());

        bool becameMixed = currentLane.AddWaste(w);
        if (becameMixed)
        {
            anim.Play("Sad");
            AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
            yield return new WaitForSeconds(info.length);
        }
        else
        {
            anim.Play("Idle");
        }

        currentState = WorkerState.Idling;
    }

    // Add held waste back to waste generator's list
    private IEnumerator DropWaste()
    {
        StopCoroutine(nameof(RadialTimerCountdown));

        wasteGenerator.AddWaste(heldWaste);
        currentState = WorkerState.WaitingForAnimation;

        yield return StartCoroutine(PlayPutDownAnimation());

        currentState = WorkerState.Resting;
    }

    private void UntargetWaste()
    {
        if (wasteToPickUp)
        {
            wasteToPickUp.hasBeenTargeted = false;
            wasteToPickUp = null;
        }
    }

    private IEnumerator PickUpWaste()
    {
        heldWaste = wasteToPickUp;
        wasteToPickUp = null;

        movement.currentVelocity = Vector3.zero;

        currentState = WorkerState.WaitingForAnimation;

        anim.Play("Pick Up");

        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(info.length);

        wasteGenerator.Remove(heldWaste);

        if (!heldWaste.hasBeenPickedUp)
        {
            //randomize when the radial timer appears
            int value = Random.Range(0, 100);
            if (value <= level.minigameChance)
            {
                ShowRadialTimer();
            }
            else
            {
                DetermineWhereToTakeWaste(heldWaste.type);
            }
        }

        heldWaste.hasBeenPickedUp = true;
        currentState = WorkerState.MovingToSkip;
    }

    public void ChangeLanesTo(int index)
	{
        movement.speedMultiplier = 1.0f;
        currentLane.RemoveWorker(this);
        currentLane = allLanes.lanes[index];
        currentLane.AddWorker(this);
        currentState = WorkerState.ChangingLane;
    }

    private void DetermineWhereToTakeWaste(Waste.Type type)
    {
        //Check type of given waste and figure out which lane it should go to
        foreach (Lane lane in allLanes.lanes)
        {
            foreach (Waste.Type acceptedWasteTypes in lane.acceptedWasteTypes)
            {
                if (acceptedWasteTypes == type)
                {
                    ChangeLanesTo(lane.laneIndex);
                    goto END; // we can't have a build of this game without a goto. it's just the way it has to be
                }
            }
        }

        END:
        movement.baseSpeed = heldWaste.speedModifier;
    }

    // For when the player hits one of the 6 buttons in the model view menu
    public void PlayerSortWaste(Waste.Type wasteType)
    {
        HideRadialTimer();
        if (wasteType != heldWaste.type)
            wasteGenerator.timesFailedMinigame++;

        DetermineWhereToTakeWaste(wasteType);
        currentState = WorkerState.MovingToSkip;
    }
}