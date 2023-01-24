using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WasteGeneration : MonoBehaviour, ISerializationCallbackReceiver
{
    //The waste gameobjects, must be set in inspector
    [SerializeField]
    private GameObject[] wasteObjects;
    //public WasteDict wasteNamesToObjects = new WasteDict();

    /// <summary>
    /// Keeps track of the generated waste objects
    /// </summary>
    private List<Waste> generatedWaste = new List<Waste>();

    /// <summary>
    /// Keeps track of how much waste exists across the entire level; waste from here gets removed ONLY when clearing through a skip
    /// </summary>
    [HideInInspector]
    public int globalWaste = 0;

    /// <summary>
    /// Boolean that determines if waste should be spawning or not
    /// </summary>
    private bool shouldSpawnWaste = true;

    [SerializeField]
    private float spawnRadius;
    
    [SerializeField, Tooltip("Determines how many waves there should be (length of array/number of elements), how long each lasts (x) and the spawn rate of waste in each wave (y)")]
    private Vector2[] waveTimesAndSpawnRates;
    private int wavesCompleted = 0; //keeps track of how many waves have passed
    private float waveTime; //time passed in current wave
    private float spawnTime; //time since last spawn

    private Text timerText;
    private float minutes;
    private float seconds;
    private Text waveText;

    [HideInInspector]
    public bool hasForeman = false;
    public Transform foremanSpawn;
    [HideInInspector]
    public int skipClearedCounter = 0;

    [HideInInspector]
    public int timesFailedMinigame = 0;

    [SerializeField]
    private int maxWaste;

    public void OnBeforeSerialize()
    {
        /* This was part of trying to get whole models to load in the 3D model view
         * The problem was that the pivot for the prefabs are set so they sit on the ground,
         * which means rotating them was weird.
         * We could have a fudge factor for each waste type i guess.......
        wasteNamesToObjects = new WasteDict
        {
            { "Plank",             wasteObjects[0] },
            { "PlankAlternate",    wasteObjects[1] },
            { "Bottle",            wasteObjects[2] },
            { "Brick",             wasteObjects[3] },
            { "BrickAlternate",    wasteObjects[4] },
            { "Clingfilm",         wasteObjects[5] },
            { "ClingfilmCrumpled", wasteObjects[6] },
            { "PlasticTub",        wasteObjects[7] },
            { "PolystyreneBox",    wasteObjects[8] },
            { "Can",               wasteObjects[9] },
            { "BentMetal",         wasteObjects[10]}
        };
        */
    }

    public void OnAfterDeserialize()
    {

    }

    private void Start()
    {
        if (waveTimesAndSpawnRates != null)
        {
            waveTime = waveTimesAndSpawnRates[0].x;
            spawnTime = waveTimesAndSpawnRates[0].y;
        }
        else
            Debug.LogWarning("NO WAVE TIMES AND SPAWN RATES SPECIFIED!"); //NOTE(irene): i just realized that even with 0 elements this warning doesn't get triggered. lol

        timerText = GameObject.Find("Timer").transform.GetChild(0).GetComponent<Text>();
        waveText = GameObject.Find("Timer").transform.GetChild(2).GetComponentInChildren<Text>();
        UpdateWaveCounter();
    }

	private void Update()
	{
        CountdownTimer();
        SpawnWaste();
    }

    private void CountdownTimer()
    {
        waveTime -= Time.deltaTime;
        if (waveTime > 0) //if there's still time before the end of the wave, update the timer
        {
            minutes = Mathf.FloorToInt(waveTime / 60);
            seconds = Mathf.FloorToInt(waveTime % 60);
            timerText.text = string.Format("{0:0}:{1:00}", minutes, seconds);
        }
        else if (wavesCompleted < waveTimesAndSpawnRates.Length) //else, go to the next wave
        {
            wavesCompleted++;
            UpdateWaveCounter();

            if (wavesCompleted >= waveTimesAndSpawnRates.Length) //if as many waves as specified in the array have passed, stop the game
            {
                shouldSpawnWaste = false;
                UpdateWaveCounter();

                Debug.Log("Game WIN!\nYou win! Good day sir!");
                timerText.text = "0:00";
                GameObject.FindWithTag("Menus").GetComponent<Menus>().WinGame();
                return;
            }
            else
                waveTime = waveTimesAndSpawnRates[wavesCompleted].x;
        }
    }

    private void UpdateWaveCounter()
    {
        waveText.text = (wavesCompleted+1).ToString() + " / " + waveTimesAndSpawnRates.Length + "     ";
    }

    /*this is in case we have time to add wave indicators
    private IEnumerator IncomingWaveIndicator()
    {
        yield return new WaitForSeconds(3f);
    } 
    */

    private void SpawnWaste()
    {
        spawnTime -= Time.deltaTime;
        if (shouldSpawnWaste && spawnTime <= 0)
        {
            spawnTime = waveTimesAndSpawnRates[wavesCompleted].y;
            globalWaste += 1;

            // Randomise the type of waste that will spawn
            int value = Random.Range(0, wasteObjects.Length);

            // Randomise the position that the waste will spawn
            Vector2 circlePoint = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPoint = transform.position + new Vector3(circlePoint.x, 0.0f, circlePoint.y);
            spawnPoint.y = 0.0f;

            generatedWaste.Add(Instantiate(wasteObjects[value], spawnPoint, Quaternion.identity).GetComponent<Waste>());
            PossiblyOverflowAlert();

            if (generatedWaste.Count > maxWaste)
            {
                GameObject.FindWithTag("Menus").GetComponent<Menus>().LoseGame(Menus.Cause.WasteGenerator);
            }
        }
    }

    public void AddWaste(Waste w)
	{
        generatedWaste.Add(w);
	}

    public Waste GetFirstFreeWaste(Lane targetLane)
	{
        foreach (Waste w in generatedWaste)
		{
            if (!w.hasBeenTargeted)
			{
                for (int i = 0; i < targetLane.acceptedWasteTypes.Length; i++) //this checks if the waste the worker is about to select even goes to the lane they're assigned to
                {
                    if (w.type == targetLane.acceptedWasteTypes[i])
                    {
                        return w;
                    }
                }
			}
		}
        return null;
	}

	public void Remove(Waste w)
	{
		generatedWaste.Remove(w);
	}

    private void PossiblyOverflowAlert()
    {
        Alerts alerts = GameObject.FindGameObjectWithTag("Alerts").GetComponent<Alerts>();
        if (generatedWaste.Count >= maxWaste / 2)
        {
            alerts.ShowGeneratorAlert();
        }
        else
        {
            alerts.HideGeneratorAlert();
        }
    }

#if UNITY_EDITOR
	private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.3f, 0.8f, 0.75f, 0.5f);
        Gizmos.DrawSphere(transform.position, 1.0f);
        
        Gizmos.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        Gizmos.DrawSphere(transform.position, spawnRadius);
    }
#endif
}
