using UnityEngine;
using UnityEngine.UI;

public class UnitButton : MonoBehaviour
{
    public enum Type
    {
        None = -1,
        Worker = 0,
        Manager = 1,
        Foreman = 2
    }

    Animation anim;
    AudioSource source;
    private bool _selectable = true;
    public bool Selectable
	{
		get
		{
            return _selectable;
		}
        set
		{
            _selectable = value;
            UpdateImage();
		}
	}

    bool _selected;
    public bool Selected
	{
        get
		{
            return _selected;
		}
        set
		{
            if (value && Selectable)
			{
                _selected = true;
			}
            else
			{
                _selected = false;
			}
            UpdateImage();
		}
	}

    void UpdateImage()
	{
        if (Selectable)
        {
            if (Selected)
			{
                transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
			}
            else
			{
                transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
			}
            Image[] images = GetComponentsInChildren<Image>();
            for (int i = 0; i < images.Length; i++)
            {
                images[i].color = Color.white;
            }
		}
        else
		{
            transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            Image[] images = GetComponentsInChildren<Image>();
            for (int i = 0; i < images.Length; i++)
            {
                images[i].color = Color.grey;
            }
        }
	}
}
