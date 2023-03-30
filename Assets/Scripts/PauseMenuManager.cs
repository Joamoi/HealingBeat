using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{
    public GameObject pauseMenu;

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
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (SceneManager.GetActiveScene().name == "WorldScene" || SceneManager.GetActiveScene().name == "XTESTWorld")
            {
                if (WalkManager.walkInstance.gameIsPaused)
                {
                    Resume();
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
                    Resume();
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
        }

        else
        {
            BeatManager.beatInstance.gameIsPaused = false;
        }

        Cursor.visible = false;
        pauseMenu.SetActive(false);
        AudioListener.pause = false;
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