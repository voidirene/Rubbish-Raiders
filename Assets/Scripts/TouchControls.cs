using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TouchControls : MonoBehaviour
{
    /// <summary>
    /// The primary object selected by tapping it
    /// </summary>
    [SerializeField]
    private Collider selected;
    private Worker selectedWorker;
    private Camera cam;
    [SerializeField]
    private float minZoom, maxZoom, zoomSpeed, speedAtMaxZoom, speedAtMinZoom;
    private Vector3 velocity;
    private BoxCollider bc;

    [SerializeField]
    private GameObject workerMale, workerFemale, workerHijab;
    [SerializeField]
    private Texture[] workerSkinTonesMale, workerSkinTonesFemale, workerHairTonesFemale;
    [SerializeField]
    private Color[] workerHijabColours;

    [SerializeField]
    private Texture[] managerSkinTones;
    [SerializeField]
    private GameObject manager;
    [SerializeField]
    private GameObject foreman;
    [HideInInspector]
    public int unitCount;
    [SerializeField]
    private int maxUnits;

    private EventSystem eventSystem;

    private GameObject modelView;
    private bool showingModelView;

    private UnitButtons unitButtons;

    private Money money;
    private Menus menus;

    private Shader standardShader, highlightShader;

    [HideInInspector]
    public Worker workerThatTriggeredModelView;

    private bool canTouch = true;


    private void Start()
    {
        cam = Camera.main;

        bc = GetComponent<BoxCollider>();

        Transform ui = transform.Find("/UI");
        eventSystem = ui.GetComponent<EventSystem>();
        money = GameObject.FindGameObjectWithTag("Money").GetComponent<Money>();
        modelView = GameObject.FindGameObjectWithTag("ModelView");
        modelView.SetActive(false);
        unitButtons = GameObject.FindGameObjectWithTag("UnitButtons").GetComponent<UnitButtons>();
        menus = GameObject.FindWithTag("Menus").GetComponent<Menus>();

        standardShader = Shader.Find("Standard");
        highlightShader = Shader.Find("TSF/BaseOutline1");

        unitCount = GameObject.FindGameObjectsWithTag("Worker").Length;
    }


    void Update()
    {
        unitButtons.buttons[(int)UnitButton.Type.Worker].Selectable  = (money.HaveEnoughMoney(Money.Action.HireWorker)  && unitCount < maxUnits);
        unitButtons.buttons[(int)UnitButton.Type.Manager].Selectable = (money.HaveEnoughMoney(Money.Action.HireManager) && unitCount < maxUnits);
        unitButtons.buttons[(int)UnitButton.Type.Foreman].Selectable = (money.HaveEnoughMoney(Money.Action.HireForeman) && unitCount < maxUnits);
        unitButtons.tally.text = unitCount + "/" + maxUnits;

        // If the mouse is clicked/the screen is tapped...
        velocity = Vector3.zero;
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            
            if (touch.phase == TouchPhase.Moved)
            {
                if (showingModelView)
                {
                    RotateModel();
                    return;
                }
                else
				{
                    velocity.x = -touch.deltaPosition.x;
                    velocity.z = -touch.deltaPosition.y;
                }
            }

            if (canTouch && touch.phase == TouchPhase.Began)
			{
                canTouch = false;
                if (HitUiElement(touch))
				{
                    return;
				}
                else
				{
                    // Do a physics raycast and work out what we hit.
                    Ray ray = cam.ScreenPointToRay(touch.position);
                    if (Physics.Raycast(ray, out RaycastHit hit))
					{
                        selected = hit.collider;
                    }
					else
					{
                        selected = null;
					}
                }
            }

            if (touch.phase == TouchPhase.Ended)
			{
                canTouch = true;

                Ray ray = cam.ScreenPointToRay(touch.position);
                if (Physics.Raycast(ray, out RaycastHit raycastHit))
				{
                    Collider hit = raycastHit.collider;

                    if (hit == selected)
					{
                        if (hit.CompareTag("Worker"))
						{
                            selectedWorker = hit.GetComponent<Worker>();
                            DeselectUnitButtons();
                            HighlightObjectsBasedOnSelected();
                            return;
						}
                        else if (hit.CompareTag("RadialTimer"))
                        {
                            UnhighlightLanes();
                            UnhighlightGenerator();

                            Worker worker = hit.GetComponentInParent<Worker>();

                            ShowModelView(worker);
                        }
                        else if (hit.CompareTag("Skip"))
						{
                            UnhighlightLanes();
                            UnhighlightGenerator();

                            hit.GetComponentInParent<Lane>().CallTruck();
                        }
                        else if (hit.CompareTag("Lane"))
                        {
                            Lane lane = hit.GetComponent<Lane>();

                            //for changing worker lanes
                            if (selectedWorker != null)
                            {
                                selectedWorker.ChangeLanesTo(lane.laneIndex);
                                UnhighlightLanes();
                            }

                            //for spawning units
                            if (unitButtons.buttons[(int)UnitButton.Type.Worker].Selected)
                            {
                                SpawnWorker(lane, raycastHit, touch);
                            }
                            else if (unitButtons.buttons[(int)UnitButton.Type.Manager].Selected)
                            {
                                SpawnManager(lane, touch);
                            }
                        }
                        else if (hit.CompareTag("WasteGenerator"))
                        {       
                            if (unitButtons.buttons[(int)UnitButton.Type.Foreman].Selectable && unitButtons.buttons[(int)UnitButton.Type.Foreman].Selected)
                            {
                                WasteGeneration wg = hit.GetComponent<WasteGeneration>();
                                SpawnForeman(wg, touch);
                            }
                        }

                        selectedWorker = null;
                    }
				}
                else
                {
                    UnhighlightLanes();
                    UnhighlightGenerator();
                    selectedWorker = null;
                    HighlightObjectsBasedOnSelected();
                }
			}
        }
        else if (Input.touchCount == 2 && !showingModelView)
		{
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);
            Vector3 pos0a = t0.position;
            Vector3 pos0b = t0.position - t0.deltaPosition;
            Vector3 pos1a = t1.position;
            Vector3 pos1b = t1.position - t1.deltaPosition;

            float zoom = Vector3.Distance(pos0a, pos1a) - Vector3.Distance(pos0b, pos1b);

            // Ignore zooms that are too little, zoom-outs when we're already as zoomed out as we can go, and zoom-ins when we're already as zoomed in as we can go.
            if ((Mathf.Abs(zoom) <= 0.01f) || ((zoom > 0) && (cam.transform.position.y <= minZoom)) || ((zoom < 0) && (cam.transform.position.y >= maxZoom)))
            {
            }
			else
			{
                cam.transform.position += Time.deltaTime * zoom * zoomSpeed * cam.transform.forward;
			}
        }

        if (!showingModelView)
        {
            float camSpeed = Mathf.Lerp(speedAtMaxZoom, speedAtMinZoom, (cam.transform.position.y - minZoom) / (maxZoom - minZoom));
        
            Vector3 position = cam.transform.position + camSpeed * Time.deltaTime * velocity;
            position.x = Mathf.Clamp(position.x, bc.bounds.min.x, bc.bounds.max.x);
            position.z = Mathf.Clamp(position.z, bc.bounds.min.z, bc.bounds.max.z);
        
            cam.transform.position = position;
        }

        // Compile out the debug cheats so the actual game doesn't spend time calculating things that will never happen.
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.A))
        {
            money.AddMoney(Money.Action.HireWorker);
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            AllLanes al = GetComponent<AllLanes>();
            al.lanes[0].CallTruck();
        }
#endif
    }

    private bool HitUiElement(Touch t)
    {
        PointerEventData ped = new PointerEventData(eventSystem);
        ped.position = t.position;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(ped, results);


        if (results.Count > 0)
        {
            GameObject uiHit = results[0].gameObject;

            // NOTE(amie): We're assuming any hit on the UI is a unit button.
            if (uiHit.TryGetComponent<UnitButton>(out UnitButton button))
            {
                bool wasSelected = button.Selected;

                DeselectUnitButtons();

                if (wasSelected)
                {
                    button.Selected = false;
                }
                else if (!wasSelected && button.Selectable)
                {
                    button.Selected = true;
                }
            }

            HighlightObjectsBasedOnSelected();
            
            return true;
        }
        return false;
    }

    #region Spawning
    void SpawnWorker(Lane lane, RaycastHit raycastHit, Touch t)
    {
        if (unitCount < maxUnits)
        {
            if (money.HaveEnoughMoney(Money.Action.HireWorker))
            {
                menus.PlaySpawnSound();
                money.DeductMoney(Money.Action.HireWorker, t);

                int sex = Random.Range(0, 3);
                Quaternion rot = Quaternion.Euler(0, 180, 0);

                if (sex == 0) // Male
                {
                    Texture skinTone = workerSkinTonesMale[Random.Range(0, workerSkinTonesMale.Length)];

                    Vector3 position = new Vector3(raycastHit.point.x, workerMale.transform.position.y, raycastHit.point.z);
                    Worker spawnedUnit = Instantiate(workerMale, position, rot, transform.Find("/Units")).GetComponent<Worker>();
                    spawnedUnit.transform.Find("Model/FranPan_Woker").GetComponent<SkinnedMeshRenderer>().material.SetTexture("_MainTex", skinTone);


                    spawnedUnit.currentLane = lane;
                    lane.workerList.Add(spawnedUnit);
                }
                else if (sex == 1) // Female
                {
                    Texture skinTone = workerSkinTonesFemale[Random.Range(0, workerSkinTonesFemale.Length)];
                    Texture hairTone = workerHairTonesFemale[Random.Range(0, workerHairTonesFemale.Length)];

                    Vector3 position = new Vector3(raycastHit.point.x, workerFemale.transform.position.y, raycastHit.point.z);
                    Worker spawnedUnit = Instantiate(workerFemale, position, rot, transform.Find("/Units")).GetComponent<Worker>();
                    spawnedUnit.transform.Find("Model/femworker1").GetComponent<SkinnedMeshRenderer>().material.SetTexture("_MainTex", skinTone);
                    spawnedUnit.transform.Find("Model/femworker1/femworker1 hair").GetComponent<SkinnedMeshRenderer>().material.SetTexture("_MainTex", hairTone);


                    spawnedUnit.currentLane = lane;
                    lane.workerList.Add(spawnedUnit);
                }
                else // Hijab, the third sex
                {
                    Color hijabColour = workerHijabColours[Random.Range(0, workerHijabColours.Length)];

                    Vector3 position = new Vector3(raycastHit.point.x, workerHijab.transform.position.y, raycastHit.point.z);
                    Worker spawnedUnit = Instantiate(workerHijab, position, rot, transform.Find("/Units")).GetComponent<Worker>();
                    spawnedUnit.transform.Find("Model/hjb").GetComponent<SkinnedMeshRenderer>().material.SetColor("_Color", hijabColour);


                    spawnedUnit.currentLane = lane;
                    lane.workerList.Add(spawnedUnit);
                }
            }
            unitCount += 1;

            if (unitCount == maxUnits)
            {
                DeselectUnitButtons();
                HighlightObjectsBasedOnSelected();
            }
        }
    }

    void SpawnManager(Lane lane, Touch t)
    {
        if (!lane.hasManager) //if the lane doesn't have a manager already
        {
            if (money.HaveEnoughMoney(Money.Action.HireManager))
            {
                menus.PlaySpawnSound();
                money.DeductMoney(Money.Action.HireManager, t);

                Texture skinTone = managerSkinTones[Random.Range(0, managerSkinTones.Length)];

                Vector3 position = lane.managerSpawn.position;
                position.y = manager.transform.position.y;
                Quaternion rotation = lane.managerSpawn.rotation;
                Manager spawnedUnit = Instantiate(manager, position, rotation, transform.Find("/Units")).GetComponent<Manager>();
                spawnedUnit.transform.Find("Manager/FranPan_manager1").GetComponent<SkinnedMeshRenderer>().material.SetTexture("_MainTex", skinTone);


                lane.AddManager(spawnedUnit);
                unitCount += 1;
            }
            UnhighlightLanes();
            HighlightLanesWithoutManager();
        }
    }
    
    void SpawnForeman(WasteGeneration wg, Touch t)
    {
        if (!wg.hasForeman) //if there is no foreman already
        {
            if (money.HaveEnoughMoney(Money.Action.HireForeman))
            {
                menus.PlaySpawnSound();
                money.DeductMoney(Money.Action.HireForeman, t);

                Texture skinTone = managerSkinTones[Random.Range(0, managerSkinTones.Length)];

                Vector3 position = wg.foremanSpawn.position;
                Quaternion rotation = wg.foremanSpawn.rotation;
                position.y = manager.transform.position.y;
                GameObject fm = Instantiate(foreman, position, rotation, transform.Find("/Units"));
                fm.transform.Find("Manager/FranPan_manager1").GetComponent<SkinnedMeshRenderer>().material.SetTexture("_MainTex", skinTone);

                wg.hasForeman = true;
            }
            unitCount += 1;
            UnhighlightGenerator();
        }
    }
    #endregion Spawning

    #region Highlighting
    // TODO: Move the following 2 functions into UnitButtons, and remove UI interaction entirely from TouchControls!!
    public void DeselectUnitButtons()
    {
        foreach (UnitButton ub in unitButtons.buttons)
        {
            ub.Selected = false;
        }
    }

    public void HighlightObjectsBasedOnSelected()
    {
        UnitButton.Type type = unitButtons.SelectedButtonType();

        switch (type)
        {
            case UnitButton.Type.Worker:
            {
                UnhighlightGenerator();
                HighlightAllLanes();
                return;
            }
            case UnitButton.Type.Manager:
            {
                UnhighlightLanes();
                UnhighlightGenerator();
                HighlightLanesWithoutManager();
                return;
            }
            case UnitButton.Type.Foreman:
            {
                UnhighlightLanes();
                HighlightGenerator();
                return;
            }
            case UnitButton.Type.None:
            {
                UnhighlightLanes();
                UnhighlightGenerator();
                break;
            }
        }

        if (selectedWorker != null)
        {
            HighlightAllLanesExcept(selectedWorker.currentLane.laneIndex);
        }
    }

    void HighlightAllLanes()
    {
        AllLanes lanes = GameObject.FindGameObjectWithTag("GameController").GetComponent<AllLanes>();

        foreach (Lane lane in lanes.lanes)
        {
            Material mat = lane.GetComponent<LineRenderer>().materials[0];
            mat.shader = highlightShader;
            mat.SetFloat(Shader.PropertyToID("_Brightness"), 1.8f);
        }
    }

    void HighlightLanesWithoutManager()
    {
        AllLanes lanes = GameObject.FindGameObjectWithTag("GameController").GetComponent<AllLanes>();

        foreach (Lane lane in lanes.lanes)
        {
            if (lane.hasManager) continue;

            Material mat = lane.GetComponent<LineRenderer>().materials[0];
            mat.shader = highlightShader;
            mat.SetFloat(Shader.PropertyToID("_Brightness"), 1.8f);
        }
    }

    void HighlightAllLanesExcept(int index)
    {
        AllLanes lanes = GameObject.FindGameObjectWithTag("GameController").GetComponent<AllLanes>();

        foreach (Lane lane in lanes.lanes)
        {
            if (lane.laneIndex == index) continue;

            Material mat = lane.GetComponent<LineRenderer>().materials[0];
            mat.shader = highlightShader;
            mat.SetFloat(Shader.PropertyToID("_Brightness"), 1.8f);
        }
    }

    public void UnhighlightLanes()
    {
        AllLanes lanes = GameObject.FindGameObjectWithTag("GameController").GetComponent<AllLanes>();
        for (int i = 0; i < lanes.lanes.Length; i += 1)
        {
            ref Material mat = ref lanes.lanes[i].GetComponent<LineRenderer>().materials[0];
            mat.shader = standardShader;
        }
    }

    void HighlightGenerator()
    {
        GameObject wg = GameObject.FindWithTag("WasteGenerator");
        wg.GetComponentInChildren<MeshRenderer>().materials[0].SetFloat(Shader.PropertyToID("_Brightness"), 1.8f);
    }

    void UnhighlightGenerator()
    {
        GameObject wg = GameObject.FindWithTag("WasteGenerator");
        wg.GetComponentInChildren<MeshRenderer>().materials[0].SetFloat(Shader.PropertyToID("_Brightness"), 1.0f);
    }
    #endregion Highlighting

    #region Model view
    public void ShowModelView(Worker worker)
	{
        workerThatTriggeredModelView = worker;
        Transform mv3d = modelView.transform.Find("ModelView3D");
        Transform am = mv3d.Find("ActualModel");
        mv3d.localRotation = Quaternion.identity;
        am.localRotation = Quaternion.identity;
        am.GetComponent<MeshRenderer>().material = worker.heldWaste.GetComponentInChildren<MeshRenderer>().material;
        am.GetComponent<MeshFilter>().mesh = worker.heldWaste.GetComponentInChildren<MeshFilter>().mesh;
        am.localPosition = worker.heldWaste.fudge;
        showingModelView = true;
        modelView.SetActive(true);
	}

    public void HideModelView()
	{
        showingModelView = false;
        modelView.SetActive(false);
	}

    private void RotateModel()
    {
        Transform viewTransform = modelView.transform.Find("ModelView3D");

        float rotSpeed = 0.1f;

        Touch t = Input.GetTouch(0);
        Vector2 modelPosDelta = new Vector2(t.deltaPosition.y, -t.deltaPosition.x) * rotSpeed;
        viewTransform.Rotate(modelPosDelta.x, modelPosDelta.y, 0, Space.World);
    }
    #endregion Model view

    #region Minigame button choices
    // NOTE(amie): This is super dumb but the alternatives are these:
    // Make a function that takes an enum, choose the enum value in the inspector, pass that to the worker. (Best)
    //      Not possible because Unity doesn't show the function if it has an enum as a parameter.
    // Make a function that takes an int.
    //      Very fragile: if enum values change we would also have to remember to do it here.
    // Make 6 dummy objects that are the prototypical "Timber", etc., then we can pass those.
    //      Having those around is stupid and it has a similiar problem to the function with an int solution.
    //
    // So that's why this is like this.
    public void ChooseTimber()
	{
        workerThatTriggeredModelView.PlayerSortWaste(Waste.Type.Timber);
        workerThatTriggeredModelView = null;
    }

    public void ChooseGlass()
	{
        workerThatTriggeredModelView.PlayerSortWaste(Waste.Type.Glass);
        workerThatTriggeredModelView = null;
    }

    public void ChooseRubble()
	{
        workerThatTriggeredModelView.PlayerSortWaste(Waste.Type.Rubble);
        workerThatTriggeredModelView = null;
    }

    public void ChoosePlastic()
	{
        workerThatTriggeredModelView.PlayerSortWaste(Waste.Type.Plastic);
        workerThatTriggeredModelView = null;
    }

    public void ChooseMetal()
	{
        workerThatTriggeredModelView.PlayerSortWaste(Waste.Type.Metal);
        workerThatTriggeredModelView = null;
    }

    public void ChooseUnrecyclable()
	{
        workerThatTriggeredModelView.PlayerSortWaste(Waste.Type.Unrecyclable);
        workerThatTriggeredModelView = null;
    }
    #endregion Minigame button choices
}
