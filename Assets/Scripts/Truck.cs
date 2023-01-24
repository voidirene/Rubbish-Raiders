using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Truck : MonoBehaviour
{
    Movement movement;
    Lane targetLane;
    Level level;
    [SerializeField]
    float waitingAtSkipTime;

    enum TruckState
    {
        MovingToLane = 0,
        WaitingAtSkip = 1,
        MovingAway = 2,
    }
    TruckState currentState;

    void Start()
    {
        movement = GetComponent<Movement>();
        currentState = TruckState.MovingToLane;
        level = GameObject.Find("GameController").GetComponent<Level>();
    }

    void Update()
    {
        switch (currentState)
        {
            case TruckState.MovingToLane:
            {
                if (movement.Pathfind(targetLane.truckSkipPosition, 0.0f))
                {
                    StartCoroutine(WaitAtSkip());
                }
                break;
            }
            case TruckState.WaitingAtSkip:
            {
                break;
            }
            case TruckState.MovingAway:
            {
                if (movement.Pathfind(level.offScreenPoint, 0.0f))
                {
                    Destroy(gameObject);
                }
                break;
            }
        }
    }

    IEnumerator WaitAtSkip()
    {
        movement.currentVelocity = Vector3.zero;
        currentState = TruckState.WaitingAtSkip;
        yield return new WaitForSeconds(waitingAtSkipTime);
        targetLane.RemoveWaste();
        currentState = TruckState.MovingAway;
        targetLane.DoneWaitingForTruck();
    }

    public void Pickup(Lane lane)
    {
        targetLane = lane;
    }

    private void OnTriggerEnter(Collider other)
    {
        level.OpenGates();
    }

    private void OnTriggerExit(Collider other)
    {
        level.CloseGates();
    }
}
