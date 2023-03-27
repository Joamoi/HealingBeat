using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public GameObject mainMenuButtons;
    public GameObject settingsButtons;

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

    public void PlayGame()
    {
        Cursor.visible = false;

        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            SceneManager.LoadScene("WorldScene");
        }

        else
        {
            SceneManager.LoadScene("XTESTWorld");
        }
    }

    public void Settings()
    {
        mainMenuButtons.SetActive(false);
        settingsButtons.SetActive(true);
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

    public void Back()
    {
        settingsButtons.SetActive(false);
        mainMenuButtons.SetActive(true);
    }

    public void ButtonAnimation(Animator animator)
    {
        animator.SetTrigger("ButtonMove");
    }
}
