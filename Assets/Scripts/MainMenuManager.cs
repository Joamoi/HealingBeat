using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MainMenuManager : MonoBehaviour
{
    public GameObject progressPrefab;

    public GameObject mainMenuButtons;
    public GameObject settingsButtons;
    public GameObject tutorialButtons;
    public GameObject logo;

    public AudioMixer audioMixer;
    public Slider musicSlider;
    public Slider effectsSlider;
    //public AudioSource testEffectSound;

    public AudioSource music;
    public float songBpm;
    private float secPerBeat;
    private float songPosInSecs;
    [HideInInspector]
    public float songPosInBeats;
    private float songStartTime;
    private float previousBeat;
    [HideInInspector]
    public bool musicPlaying = false;
    public Volume volume;
    private ChromaticAberration chroAb;
    public float chroAbIntensity;
    public float lightDuration;

    void Start()
    {
        if (GameObject.FindGameObjectsWithTag("Progress").Length == 0)
        {
            Instantiate(progressPrefab);
        }

        ProgressManager progressManager = GameObject.FindGameObjectsWithTag("Progress")[0].GetComponent<ProgressManager>();
        progressManager.previousScene = "MainMenu";

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

        secPerBeat = 60f / songBpm;
        previousBeat = -1;
        StartCoroutine("StartMusic");
    }

    void Update()
    {
        // beat is based on dsptime which is more accurate
        songPosInSecs = (float)(AudioSettings.dspTime - songStartTime);
        songPosInBeats = songPosInSecs / secPerBeat;

        if ((songPosInBeats - previousBeat) >= 1f && musicPlaying)
        {
            previousBeat++;
            Shader.SetGlobalFloat("_FlipBookTile", previousBeat);

            StartCoroutine("PPIntensify");
        }
    }

    IEnumerator StartMusic()
    {
        yield return new WaitForSeconds(0.5f);

        // record the time when the music starts
        songStartTime = (float)AudioSettings.dspTime;
        music.Play();
        musicPlaying = true;
    }

    IEnumerator PPIntensify()
    {
        volume.profile.TryGet<ChromaticAberration>(out chroAb);

        chroAb.intensity.value += chroAbIntensity;

        yield return new WaitForSeconds(lightDuration / 2f);

        chroAb.intensity.value -= 0.5f * chroAbIntensity;

        yield return new WaitForSeconds(lightDuration / 2f);

        chroAb.intensity.value -= 0.5f * chroAbIntensity;
    }

    public void PlayGame()
    {
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

    public void SettingsBack()
    {
        settingsButtons.SetActive(false);
        mainMenuButtons.SetActive(true);
    }

    public void Tutorial()
    {
        logo.SetActive(false);
        mainMenuButtons.SetActive(false);
        tutorialButtons.SetActive(true);
    }

    public void TutorialBack()
    {
        tutorialButtons.SetActive(false);
        logo.SetActive(true);
        mainMenuButtons.SetActive(true);
    }

    public void ButtonAnimation(Animator animator)
    {
        animator.SetTrigger("ButtonMove");
    }
}
