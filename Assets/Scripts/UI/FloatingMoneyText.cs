using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatingMoneyText : MonoBehaviour
{
    Vector3 originalPosition;
    Text text;
    string s;
    bool positive;

    void Start()
    {
        text = GetComponent<Text>();
        text.text = s;
        if (positive)
        {
            text.color = new Color(0.2f, 0.5f, 0.2f, 1.0f);
        }
        else
        {
            text.color = new Color(0.76f, 0.3f, 0.3f, 1.0f);
        }
        originalPosition = transform.position;
        StartCoroutine(Disappear());
    }

    private void OnEnable()
    {
        transform.position = originalPosition;
    }

    private void LateUpdate()
    {
        Vector3 tempPos = transform.position;
        tempPos.y -= Time.deltaTime * 100f;
        transform.position = tempPos;

        Color c = text.color;
        c.a -= Time.deltaTime * 1.0f;
        text.color = c;
    }

    private IEnumerator Disappear()
    {
        yield return new WaitForSeconds(1.5f);

        Destroy(gameObject);
    }

    public void SetString(bool positiveSign, string toSet)
    {
        if (positiveSign)
        {
            s = "+" + toSet;
            positive = true;
        }
        else
        {
            s = "-" + toSet;
            positive = false;
        }
    }
}
