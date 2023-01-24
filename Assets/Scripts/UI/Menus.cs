using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menus : MonoBehaviour
{
    [SerializeField]
    private GameObject winModal;
    [SerializeField]
    private GameObject loseModal;

    [SerializeField]
    private AudioClip buttonClick, spawnSound;
    [SerializeField]
    private AudioClip[] musicClips;

    private AudioSource sfx;
    private AudioSource[] ambience;

    [SerializeField, Tooltip("A higher value means quieter nature sounds compared to the industry sounds")]
    private float natureSoundsAttentuation = 0.9f;

    [SerializeField]
    private Sprite[] characters;

    private static Menus instance;
    private void Awake()
    {
        // Prevent duplicates
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }

        GameObject.DontDestroyOnLoad(this.gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        AudioSource[] sources = GetComponents<AudioSource>();
        sfx = sources[0];
        ambience = new AudioSource[3];
        ambience[0] = sources[1];
        ambience[1] = sources[2];
        ambience[2] = sources[3];

        ambience[0].clip = musicClips[0];
        ambience[1].clip = musicClips[1];
        ambience[2].clip = musicClips[2];

        float sfxVolume = PlayerPrefs.GetFloat("sfxvolume", 1);
        float ambienceVolume = PlayerPrefs.GetFloat("ambiencevolume", 1);

        sfx.volume = sfxVolume;
        // 0 and 1 are the nature sounds, they should be quieter than the rest
        ambience[0].volume = (1 - natureSoundsAttentuation) * ambienceVolume;
        ambience[1].volume = (1 - natureSoundsAttentuation) * ambienceVolume;
        ambience[2].volume = ambienceVolume;
    }

    // Yes this is actually necessary.
    private void SetupMenu()
    {
        Transform canvas = GameObject.Find("/UI/Canvas/").transform;
        RectTransform character = canvas.Find("StartMenu/Character") as RectTransform;
        
        Transform settings = canvas.Find("Settings");
        Slider sfxSlider = settings.Find("SFX Slider").GetComponent<Slider>();
        sfxSlider.value = sfx.volume;
        sfxSlider.onValueChanged.AddListener(ChangeSFXVolume);
        Slider ambienceSlider = settings.Find("Ambience Slider").GetComponent<Slider>();
        ambienceSlider.value = ambience[2].volume;
        ambienceSlider.onValueChanged.AddListener(ChangeAmbienceVolume);
        Button button = settings.Find("BackButton").GetComponent<Button>();
        button.onClick.AddListener(PlayButtonClick);
        button.onClick.AddListener(() => StartCoroutine(MoveCharacter(character)));

        settings.gameObject.SetActive(false);

        canvas.Find("StartMenu/StartButton").GetComponent<Button>().onClick.AddListener(PlayButtonClick);
        canvas.Find("StartMenu/SettingsButton").GetComponent<Button>().onClick.AddListener(PlayButtonClick);
        StartCoroutine(MoveCharacter(character));

        button = canvas.Find("LevelSelect/LevelButtons/Level1Button").GetComponent<Button>();
        button.onClick.AddListener(PlayButtonClick);
        button.onClick.AddListener(() => LoadScene("Level1"));
        button = canvas.Find("LevelSelect/LevelButtons/Level2Button").GetComponent<Button>();
        button.onClick.AddListener(PlayButtonClick);
        button.onClick.AddListener(() => LoadScene("Level2"));
        button = canvas.Find("LevelSelect/LevelButtons/Level3Button").GetComponent<Button>();
        button.onClick.AddListener(PlayButtonClick);
        button.onClick.AddListener(() => LoadScene("Level3"));
        button = canvas.Find("LevelSelect/LevelButtons/Level4Button").GetComponent<Button>();
        button.onClick.AddListener(PlayButtonClick);
        button.onClick.AddListener(() => LoadScene("Level4"));
        button = canvas.Find("LevelSelect/LevelButtons/Level5Button").GetComponent<Button>();
        button.onClick.AddListener(PlayButtonClick);
        button.onClick.AddListener(() => LoadScene("Level5"));
        button = canvas.Find("LevelSelect/BackButton").GetComponent<Button>();
        button.onClick.AddListener(PlayButtonClick);
        button.onClick.AddListener(() => StartCoroutine(MoveCharacter(character)));
        canvas.Find("LevelSelect/HowtoButton").GetComponent<Button>().onClick.AddListener(PlayButtonClick);

        canvas.Find("Tutorial/Page1/Next").GetComponent<Button>().onClick.AddListener(PlayButtonClick);
        canvas.Find("Tutorial/Page1/Back").GetComponent<Button>().onClick.AddListener(PlayButtonClick);
        canvas.Find("Tutorial/Page2/Next").GetComponent<Button>().onClick.AddListener(PlayButtonClick);
        canvas.Find("Tutorial/Page2/Prev").GetComponent<Button>().onClick.AddListener(PlayButtonClick);
        canvas.Find("Tutorial/Page3/Next").GetComponent<Button>().onClick.AddListener(PlayButtonClick);
        canvas.Find("Tutorial/Page3/Prev").GetComponent<Button>().onClick.AddListener(PlayButtonClick);
        canvas.Find("Tutorial/Page4/Next").GetComponent<Button>().onClick.AddListener(PlayButtonClick);
        canvas.Find("Tutorial/Page4/Prev").GetComponent<Button>().onClick.AddListener(PlayButtonClick);
        canvas.Find("Tutorial/Page5/Prev").GetComponent<Button>().onClick.AddListener(PlayButtonClick);
        canvas.Find("Tutorial/Page5/Back").GetComponent<Button>().onClick.AddListener(PlayButtonClick);
    }

    IEnumerator MoveCharacter(RectTransform character)
    {
        character.GetComponent<Image>().sprite = characters[Random.Range(0, characters.Length)];
        float t = 0;
        float totalTime = 0.2f;
        Vector2 startingPos = new Vector2(1000, character.anchoredPosition.y);
        Vector2 finalPos = new Vector2(280, startingPos.y);

        while (t < totalTime)
        {
            character.anchoredPosition = Vector2.Lerp(startingPos, finalPos, (t / totalTime));
            t += Time.deltaTime;
            yield return null;
        }
        character.anchoredPosition = finalPos;
    }

    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "Level1":
            case "Level2":
            case "Level3":
            case "Level4":
            case "Level5":
            {
                foreach (var m in ambience)
                {
                    m.Play();
                }
                break;
            }
            case "Menu":
            {
                foreach (var m in ambience)
                {
                    m.Stop();
                }
                SetupMenu();
                break;
            }
            default:
            {
                throw new System.NotImplementedException();
            }
        }
    }

    public void ChangeSFXVolume(float vol)
    {
        sfx.volume = vol;
        PlayerPrefs.SetFloat("sfxvolume", vol);
        PlayerPrefs.Save();
    }

    public void ChangeAmbienceVolume(float vol)
    {
        ambience[0].volume = vol - natureSoundsAttentuation;
        ambience[1].volume = vol - natureSoundsAttentuation;
        ambience[2].volume = vol;
        PlayerPrefs.SetFloat("ambiencevolume", vol);
        PlayerPrefs.Save();
    }

    public void PlayButtonClick()
    {
        sfx.clip = buttonClick;
        sfx.pitch = Random.Range(0.8f, 1.2f);
        sfx.Play();
    }

    public void PlaySpawnSound()
    {
        sfx.clip = spawnSound;
        sfx.pitch = Random.Range(0.8f, 1.2f);
        sfx.Play();
    }

    public void WinGame()
    {
        TouchControls tc = GameObject.FindGameObjectWithTag("GameController").GetComponent<TouchControls>();
        tc.HideModelView();

        //grading system
        Money money = GameObject.FindGameObjectWithTag("Money").GetComponent<Money>(); //get the money manager
        int grade = money.CalculatePlayerGrade();
        Image[] stars = winModal.GetComponentsInChildren<Image>();
        for (int i = 0; i < grade; i++)
        {
            stars[i+1].enabled = true; //note: the +1 is to offset for the fact that the winmodal has an image component itself
        }

        //stats
        Text text = winModal.GetComponentInChildren<Text>();
        text.text = money.amountHeld + "\n" + tc.unitCount + "\n" + (money.initialBudget - money.amountHeld) + "\n" + GameObject.FindGameObjectWithTag("WasteGenerator").GetComponent<WasteGeneration>().timesFailedMinigame;
        
        //stop everything else
        Instantiate(winModal, transform.Find("/UI/Canvas"));
        Time.timeScale = 0;

        foreach (AudioSource s in ambience)
        {
            s.Stop();
        }
    }

    public enum Cause
    {
        WasteGenerator = 0,
        Skip = 1,
    }
    readonly Dictionary<Cause, string> causes = new Dictionary<Cause, string>
    {
        { Cause.WasteGenerator, "Too much unsorted\nwaste!" },
        { Cause.Skip, "A skip overflowed!" },
    };

    /// <summary>
    /// Function for triggering Game Over
    /// </summary>
    public void LoseGame(Cause cause)
    {
        TouchControls tc = GameObject.FindGameObjectWithTag("GameController").GetComponent<TouchControls>();
        tc.HideModelView();

        Text text = loseModal.GetComponentInChildren<Text>();
        text.text = causes[cause];
        Instantiate(loseModal, transform.Find("/UI/Canvas"));
        Time.timeScale = 0;

        foreach (AudioSource s in ambience)
        {
            s.Stop();
        }
    }

    public void PauseGame()
    {
        foreach (AudioSource s in ambience)
        {
            s.volume /= 2;
        }
        Time.timeScale = 0;
    }

    public void UnpauseGame()
    {
        foreach (AudioSource s in ambience)
        {
            s.volume *= 2;
        }
        Time.timeScale = 1;
    }
}
