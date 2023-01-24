using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    [Tooltip("This works as a percentage, if you input 10 then it's 10% chance of a minigame popping up")]
    public float minigameChance;

    public Vector3 offScreenPoint;

    [SerializeField]
    Animator gates;

    public void OpenGates()
    {
        gates.Play("Opening");
    }

    public void CloseGates()
    {
        gates.Play("Closing");
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(offScreenPoint, new Vector3(1.0f, 1.0f, 1.0f));
    }
#endif
}
