using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnpauseButton : ButtonClickSound
{
    public void Unpause()
    {
        Click();
        transform.parent.gameObject.SetActive(false);
        GameObject.FindWithTag("Menus").GetComponent<Menus>().UnpauseGame();
    }
}
