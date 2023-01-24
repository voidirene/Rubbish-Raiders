using System.Collections.Generic;
using UnityEngine;

public class Waste : MonoBehaviour
{
    public bool hasBeenTargeted = false; // to stop other people from picking up the same waste
    public bool hasBeenPickedUp = false; // to prevent minigames from happening on waste that's already been decided on.

    public enum Type {
        Timber = 0,
        Glass = 1,
        Rubble = 2,
        Plastic = 3,
        Metal = 4,
        Unrecyclable = 5
    };
    public Type type;

    public float speedModifier;

    [SerializeField]
    public Vector3 fudge;
}
