using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldBoss : MonoBehaviour
{
    public LayerMask movePoint;

    // Update is called once per frame
    void Update()
    {
        if (Physics.OverlapSphere(transform.position, 0.2f, movePoint).Length > 0)
        {
            if(SceneManager.GetActiveScene().name == "WorldScene")
            {
                ProgressManager progressManager = GameObject.FindGameObjectsWithTag("Progress")[0].GetComponent<ProgressManager>();
                progressManager.bossReached = true;

                SceneManager.LoadScene("BattleScene");
            }

            else
            {
                ProgressManager progressManager = GameObject.FindGameObjectsWithTag("Progress")[0].GetComponent<ProgressManager>();
                progressManager.bossReached = true;

                SceneManager.LoadScene("XTESTBattle");
            }
        }
    }
}
