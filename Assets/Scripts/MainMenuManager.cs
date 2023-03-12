using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject mainMenuButtons;
    public GameObject settingsButtons;

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

    public void SetMusicVolume()
    {

    }

    public void SetEffectsVolume()
    {

    }

    public void Back()
    {
        settingsButtons.SetActive(false);
        mainMenuButtons.SetActive(true);
    }
}
