using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndManager : MonoBehaviour
{
    public GameObject buttons;
    public GameObject progressPrefab;
    public GameObject fadeImage;
    public GameObject endText;
    public Animator textAnimator;

    void Start()
    {
        Cursor.visible = false;

        if (GameObject.FindGameObjectsWithTag("Progress").Length == 0)
        {
            Instantiate(progressPrefab);
        }

        ProgressManager progressManager = GameObject.FindGameObjectsWithTag("Progress")[0].GetComponent<ProgressManager>();
        progressManager.previousScene = "EndScene";

        fadeImage.SetActive(true);
        StartCoroutine("ShowButtons");
    }

    public void ToMainMenu()
    {
        ProgressManager progressManager = GameObject.FindGameObjectsWithTag("Progress")[0].GetComponent<ProgressManager>();
        progressManager.bossReached = false;
        progressManager.resetNPCs = true;

        if (SceneManager.GetActiveScene().name == "EndScene")
        {
            SceneManager.LoadScene("MainMenu");
        }

        else
        {
            SceneManager.LoadScene("XTESTMenu");
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    IEnumerator ShowButtons()
    {
        yield return new WaitForSeconds(2f);
        endText.SetActive(true);
        textAnimator.SetTrigger("FadeIn");
        yield return new WaitForSeconds(3f);
        Cursor.visible = true;
        buttons.SetActive(true);
    }

    public void ButtonAnimation(Animator animator)
    {
        animator.SetTrigger("ButtonMove");
    }
}
