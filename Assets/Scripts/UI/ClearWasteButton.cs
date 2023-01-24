using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearWasteButton : MonoBehaviour
{
    Vector3 originalPosition;


    private BoxCollider col;
    private SpriteRenderer spriteRenderer;

    //for floating
    [SerializeField]
    private float height = 0.1f;
    [SerializeField]
    private float interval = 1f;
    Vector3 tempPos = new Vector3();

    void Start()
    {
        originalPosition = transform.position;

        col = transform.parent.GetComponent<BoxCollider>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (col.enabled)
        {
            tempPos = originalPosition;
            tempPos.y += Mathf.Cos(Time.fixedTime * Mathf.PI * interval) * height;

            transform.position = tempPos;


            if (!spriteRenderer.enabled)
                spriteRenderer.enabled = true;
        }
    }

    public void UpdateSprite(Sprite clearWasteSprite, Sprite contaminatedWasteSprite, bool isMixedWaste)
    {
        if (isMixedWaste)
            spriteRenderer.sprite = contaminatedWasteSprite;
        else
            spriteRenderer.sprite = clearWasteSprite;
    }

    public void Enable()
    {
        col.enabled = true;
        spriteRenderer.enabled = true;
    }

    public void Disable()
    {
        col.enabled = false;
        spriteRenderer.enabled = false;
    }

    public void TurnRed()
    {
        GetComponent<SpriteRenderer>().color = Color.red;
    }

    public void TurnGreen()
    {
        GetComponent<SpriteRenderer>().color = Color.white;
    }
}
