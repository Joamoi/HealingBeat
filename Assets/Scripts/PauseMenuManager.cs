using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject pauseButtons;
    public GameObject tutorialButtons;
    private bool inTutorial = false;

    public AudioMixer audioMixer;
    public Slider musicSlider;
    public Slider effectsSlider;
    //public AudioSource testEffectSound;

    void Start()
    {
        if (!(PlayerPrefs.GetFloat("musicVol") == 0))
        {
            audioMixer.SetFloat("musicVolume", Mathf.Log10(PlayerPrefs.GetFloat("musicVol")) * 20);
            musicSlider.value = (PlayerPrefs.GetFloat("musicVol"));
        }

        if (!(PlayerPrefs.GetFloat("effectsVol") == 0))
        {
            audioMixer.SetFloat("effectsVolume", Mathf.Log10(PlayerPrefs.GetFloat("effectsVol")) * 20);
            effectsSlider.value = (PlayerPrefs.GetFloat("effectsVol"));
        }
    }

    void Update()
    {
        // escape button pauses/resumes game
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (SceneManager.GetActiveScene().name == "WorldScene" || SceneManager.GetActiveScene().name == "XTESTWorld")
            {
                if (WalkManager.walkInstance.gameIsPaused)
                {
                    if (inTutorial)
                    {
                        Back();
                    }
                    else
                    {
                        Resume();
                    }
                }
                else
                {
                    Pause();
                }
            }

            else if (SceneManager.GetActiveScene().name == "BattleScene" || SceneManager.GetActiveScene().name == "XTESTBattle")
            {
                if (BeatManager.beatInstance.gameIsPaused)
                {
                    if (inTutorial)
                    {
                        Back();
                    }
                    else
                    {
                        Resume();
                    }
                }
                else
                {
                    Pause();
                }
            }
        }

    }

    public void Pause()
    {
        if (SceneManager.GetActiveScene().name == "WorldScene" || SceneManager.GetActiveScene().name == "XTESTWorld")
        {
            WalkManager.walkInstance.gameIsPaused = true;

            if (WalkManager.walkInstance.moveTextInUse)
            {
                WalkManager.walkInstance.moveText.SetActive(false);
            }

            ProgressManager progressManager = GameObject.FindGameObjectsWithTag("Progress")[0].GetComponent<ProgressManager>();
            if (progressManager.npcsAmount == progressManager.npcsLeft && WalkManager.walkInstance.battleOn)
            {
                WalkManager.walkInstance.leftBunny.enabled = false;
                WalkManager.walkInstance.rightBunny.enabled = false;
                WalkManager.walkInstance.leftBunnyIdle.enabled = false;
                WalkManager.walkInstance.rightBunnyIdle.enabled = false;
                WalkManager.walkInstance.leftBunnyText.SetActive(false);
                WalkManager.walkInstance.rightBunnyText.SetActive(false);
                WalkManager.walkInstance.leftBunnyGlow.SetActive(false);
                WalkManager.walkInstance.rightBunnyGlow.SetActive(false);
                WalkManager.walkInstance.takeYourTimeText.SetActive(false);
            }
        }

        else
        {
            BeatManager.beatInstance.gameIsPaused = true;
        }

        AudioListener.pause = true;
        pauseMenu.SetActive(true);
        Cursor.visible = true;
    }

    public void Resume()
    {
        if (SceneManager.GetActiveScene().name == "WorldScene" || SceneManager.GetActiveScene().name == "XTESTWorld")
        {
            WalkManager.walkInstance.gameIsPaused = false;

            if (WalkManager.walkInstance.moveTextInUse)
            {
                WalkManager.walkInstance.moveText.SetActive(true);
            }

            ProgressManager progressManager = GameObject.FindGameObjectsWithTag("Progress")[0].GetComponent<ProgressManager>();
            if (progressManager.npcsAmount == progressManager.npcsLeft && WalkManager.walkInstance.battleOn)
            {
                WalkManager.walkInstance.StartRandomBunny();
            }
        }

        else
        {
            BeatManager.beatInstance.gameIsPaused = false;
        }

        Cursor.visible = false;
        pauseMenu.SetActive(false);
        AudioListener.pause = false;
    }

    public void Tutorial()
    {
        pauseButtons.SetActive(false);
        tutorialButtons.SetActive(true);
        inTutorial = true;
    }

    public void Back()
    {
        tutorialButtons.SetActive(false);
        pauseButtons.SetActive(true);
        inTutorial = false;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    // sliders in settings control the volumes in audiomixer

    public void SetMusicVolume(float volume)
    {
        // slider would be logaritmic without the fix
        audioMixer.SetFloat("musicVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("musicVol", volume);
    }

    public void SetEffectsVolume(float volume)
    {
        // slider would be logaritmic without the fix
        audioMixer.SetFloat("effectsVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("effectsVol", volume);

        //testEffectSound.Play(0);
    }

    public void ButtonAnimation(Animator animator)
    {
        animator.SetTrigger("ButtonMove");
    }
}