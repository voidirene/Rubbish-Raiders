using UnityEngine;

public class AllLanes : MonoBehaviour
{
    public Lane[] lanes;

    private void Awake() //added this so we don't have to assign lanes manually
	{
		GameObject[] laneObjects = GameObject.FindGameObjectsWithTag("Lane");
		for (int i = 0; i < laneObjects.Length; i++)
		{
			lanes[i] = laneObjects[i].GetComponent<Lane>();
		}
	}

    private void Start()
	{
		for (int laneIndex = 0; laneIndex < lanes.Length; laneIndex += 1)
		{
			lanes[laneIndex].Setup(laneIndex);
		}
	}
}