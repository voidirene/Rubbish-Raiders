using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonClickSound : MonoBehaviour
{
    public void Click()
    {
        GameObject.FindWithTag("Menus").GetComponent<Menus>().PlayButtonClick();
    }
}
