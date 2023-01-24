using UnityEngine;
using System.Collections.Generic;

public class Movement : MonoBehaviour
{
    [HideInInspector]
    public Vector3 currentVelocity;
    [Tooltip("How quickly a unit will move steer towards its target (it's not exactly that though).")]
    private float maxForce = 10.0f;
    public float baseSpeed = 1.0f;
    [HideInInspector]
    public float speedMultiplier = 1.0f; // Received from the Supervisor
    [SerializeField, Tooltip("How fast units turn visually.")]
    private float angularSpeed = 2.0f;

    private NavGraph navGraph;
    private List<Vector3> currentPath = null;
    private Vector3 currentTarget;
    private int currentIndex = 0;

    private void Start()
	{
        navGraph = GameObject.Find("GameController").GetComponent<NavGraph>();
	}

    private void Update()
    {
        transform.position += currentVelocity * Time.deltaTime;
    }

    // Based on (Homatash, 2021, Steering Behaviours)
    /// <summary>
    /// Seek towards a target.
    /// </summary>
    /// <returns>
    /// True when the target has been reached, false otherwise.
    /// </returns>
    private bool SteerTowards(Vector3 target)
    {
        Vector3 targetPosition = new Vector3(target.x, transform.position.y, target.z);
        Vector3 targetVector = targetPosition - transform.position;

        float singleStep = angularSpeed * Time.deltaTime;
        Vector3 lookDirection = Vector3.RotateTowards(transform.forward, targetVector, singleStep, 0.0f);
        transform.rotation = Quaternion.LookRotation(lookDirection);

        // NOTE(amie): Tweak this value to set how close to a target is "close enough" to be considered "arrived" at the destination.
        float epsilon = 0.4f * baseSpeed;

        float distanceToTarget = targetVector.magnitude;
        if (distanceToTarget < epsilon)
        {
            return true;
        }

        float strength = 5.0f;
        Vector3 desiredVelocity = targetVector.normalized * strength;
        Vector3 steeringForce = desiredVelocity - currentVelocity;
        steeringForce = Vector3.ClampMagnitude(steeringForce, maxForce);
        currentVelocity += steeringForce;
        currentVelocity *= (baseSpeed * speedMultiplier);

        return false;
    }

    /// <summary>
    /// Get a path from the current position to the target position along the NavGraph, then moves along that path on subsequent calls.
    /// </summary>
    /// <param name="target">The position to move to. The path will stop at the NavNode nearest to the target.</param>
    /// <param name="radius">The target will be randomly set to a point this far away from the target.</param>
    /// <returns>Whether the worker has arrived at the NavNode nearest to the target.</returns>
    public bool Pathfind(Vector3 target, float radius)
    {
        if (currentPath == null)
        {
            Vector2 circlePoint = radius * Random.insideUnitCircle;
            currentTarget = target + new Vector3(circlePoint.x, 0.0f, circlePoint.y);
            currentPath = navGraph.GetPath(transform.position, currentTarget);
            currentPath[currentPath.Count-1] = currentTarget;
            if (currentPath.Count > 1)
            {
                currentIndex = 1;
            }
        }

        if (SteerTowards(currentPath[currentIndex]))
        {
            currentIndex += 1;
        }

        bool arrived = currentIndex >= currentPath.Count;
        if (arrived)
		{
            currentIndex = 0;
            currentPath = null;
		}
        return arrived;
    }
}