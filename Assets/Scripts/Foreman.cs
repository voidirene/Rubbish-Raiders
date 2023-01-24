using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Foreman : MonoBehaviour
{
    [SerializeField, Tooltip("This reduces the minigame chance percentage by a flat amount. Ex. if default chance is 33%, a 30 reduction will turn the chance to 3%")]
    private float minigameChanceReduction = 30;

    private Level level;

    private void Start()
    {
        level = GameObject.Find("GameController").GetComponent<Level>();
        level.minigameChance -= minigameChanceReduction;
    }
}
