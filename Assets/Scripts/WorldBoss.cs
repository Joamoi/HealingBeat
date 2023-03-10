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
                SceneManager.LoadScene("BattleScene");
            }

            else
            {
                SceneManager.LoadScene("XTESTBattle");
            }
        }
    }
}
