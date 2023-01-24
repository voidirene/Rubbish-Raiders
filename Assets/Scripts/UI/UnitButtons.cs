using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitButtons : MonoBehaviour, ISerializationCallbackReceiver
{
    public UnitButton[] buttons;
    public Text tally;
    private bool hidden = true;
    [SerializeField]
    private GameObject arrow;
    private Animator[] anims;

    public void OnBeforeSerialize()
    {
        Transform first = transform.Find("WorkerButton");
        Transform second = transform.Find("ManagerButton");
        Transform third = transform.Find("ForemanButton");

        if (third != null)
        {
            buttons = new UnitButton[3];
            buttons[(int)UnitButton.Type.Worker] = first.GetComponent<UnitButton>();
            buttons[(int)UnitButton.Type.Manager] = second.GetComponent<UnitButton>();
            buttons[(int)UnitButton.Type.Foreman] = third.GetComponent<UnitButton>();
        }
        else
        {
            buttons = new UnitButton[2];
            buttons[(int)UnitButton.Type.Worker] = first.GetComponent<UnitButton>();
            buttons[(int)UnitButton.Type.Manager] = second.GetComponent<UnitButton>();
        }

        tally = transform.Find("UnitTally").GetComponentInChildren<Text>();
    }

    public void OnAfterDeserialize()
    {

    }

    private void Start()
    {
        anims = GetComponentsInChildren<Animator>();
    }

    public void ShowOrHide()
    {
        if (Time.timeScale == 1)
        {
            if (hidden)
            {
                RectTransform hideButtonRectTransform = GetComponent<RectTransform>();
                StartCoroutine(MoveOverTime(hideButtonRectTransform, new Vector2(200, hideButtonRectTransform.anchoredPosition.y), 0.2f));
                arrow.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 180);
                foreach (Animator a in anims)
                {
                    a.Play("Opening");
                }
            }
            else
            {
                RectTransform hideButtonRectTransform = GetComponent<RectTransform>();
                StartCoroutine(MoveOverTime(hideButtonRectTransform, new Vector2(50, hideButtonRectTransform.anchoredPosition.y), 0.2f));
                arrow.GetComponent<RectTransform>().rotation = Quaternion.identity;

                TouchControls tc = GameObject.Find("GameController").GetComponent<TouchControls>();
                tc.DeselectUnitButtons();
                tc.HighlightObjectsBasedOnSelected();
            }

        }
    }

    private IEnumerator MoveOverTime(RectTransform objectRectTransform, Vector2 targetLocation, float timeToReachTarget)
    {
        float t = 0;
        Vector2 startingPos = objectRectTransform.anchoredPosition;

        while (t < timeToReachTarget)
        {
            objectRectTransform.anchoredPosition = Vector2.Lerp(startingPos, targetLocation, (t / timeToReachTarget));
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        objectRectTransform.anchoredPosition = targetLocation;
        hidden = !hidden;
        
        if (hidden)
        {
            foreach (Animator a in anims)
            {
                a.Play("Idle");
            }
        }
    }

    public UnitButton.Type SelectedButtonType()
    {
        if (buttons[0].Selected) return UnitButton.Type.Worker;
        else if (buttons[1].Selected) return UnitButton.Type.Manager;
        else if (buttons[2].Selected) return UnitButton.Type.Foreman;
        else return UnitButton.Type.None;
    }
}
