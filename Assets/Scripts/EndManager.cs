using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndManager : MonoBehaviour
{
    public GameObject buttons;

    void Start()
    {
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
        yield return new WaitForSeconds(3f);
        Cursor.visible = true;
        buttons.SetActive(true);
    }

    public void ButtonAnimation(Animator animator)
    {
        animator.SetTrigger("ButtonMove");
    }
}
