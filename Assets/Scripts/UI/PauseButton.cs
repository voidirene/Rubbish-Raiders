using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseButton : ButtonClickSound
{
    public void Pause()
    {
        if (Time.timeScale == 1f)
        {
            Click();
            transform.Find("PauseMenu").gameObject.SetActive(true);
            GameObject.FindWithTag("Menus").GetComponent<Menus>().PauseGame();
        }
    }
}
