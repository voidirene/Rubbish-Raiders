using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alerts : MonoBehaviour
{
    [SerializeField]
    private GameObject alertObject; //a reference to the alert prefab
    private GameObject[] alerts; //the actual instances
    private List<int> activeAlerts; // the indices in the alerts array which are being shown
    private Vector3[] targetPositions; // the targets for the alerts
    private Camera cam;

    private void Start()
    {
        //create an alert for each lane
        Lane[] lanes = GameObject.FindGameObjectWithTag("GameController").GetComponent<AllLanes>().lanes;

        alerts = new GameObject[lanes.Length + 1];
        targetPositions = new Vector3[lanes.Length + 1];
        activeAlerts = new List<int>(lanes.Length + 1);
        for (int i = 0; i < lanes.Length; i++)
        {
            alerts[i] = Instantiate(alertObject, transform);
            alerts[i].SetActive(false);
            targetPositions[i] = lanes[i].workerSkipPosition;
        }

        alerts[alerts.Length-1] = Instantiate(alertObject, transform);
        alerts[alerts.Length-1].SetActive(false);
        targetPositions[alerts.Length-1] = GameObject.FindWithTag("WasteGenerator").transform.position;

        cam = Camera.main;
    }

    public void ShowLaneAlert(Lane lane)
    {
        activeAlerts.Add(lane.laneIndex);

        ClearWasteButton[] clearWasteButtons = lane.GetComponentsInChildren<ClearWasteButton>();
        for (int i = 0; i < clearWasteButtons.Length; i++)
        {
            clearWasteButtons[i].TurnRed();
        }
    }

    public void ShowGeneratorAlert()
    {
        activeAlerts.Add(alerts.Length-1);
    }

    public void HideAlert(Lane lane)
    {
        ClearWasteButton[] clearWasteButtons = lane.GetComponentsInChildren<ClearWasteButton>();
        for (int i = 0; i < clearWasteButtons.Length; i++)
        {
            clearWasteButtons[i].TurnGreen();
        }

        alerts[lane.laneIndex].SetActive(false);
        activeAlerts.Remove(lane.laneIndex);
    }

    public void HideGeneratorAlert()
    {
        alerts[alerts.Length-1].SetActive(false);
        activeAlerts.Remove(alerts.Length-1);
    }

    private void Update()
    {
        float borderOffset = 100.0f;

        foreach (int index in activeAlerts)
        {
            Vector2 targetScreenPoint = cam.WorldToScreenPoint(targetPositions[index]);
            Rect screen = new Rect(0, 0, Screen.width, Screen.height);
            
            if (!screen.Contains(targetScreenPoint))
            {
                alerts[index].SetActive(true);
                targetScreenPoint.x = Mathf.Clamp(targetScreenPoint.x, borderOffset, Screen.width - borderOffset);
                targetScreenPoint.y = Mathf.Clamp(targetScreenPoint.y, borderOffset, Screen.height - borderOffset);

                alerts[index].GetComponent<RectTransform>().position = targetScreenPoint;
            }
            else
            {
                alerts[index].SetActive(false);
            }
        }
    }
}
