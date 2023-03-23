using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldNpc : MonoBehaviour
{
    public float hp = 100f;
    public float damageTakenPerHit = 7f;
    public GameObject testCloud;
    public GameObject reaction;

    // check from progressmanager if this npc is already healed or not
    void Start()
    {
        ProgressManager progressManager = GameObject.FindGameObjectsWithTag("Progress")[0].GetComponent<ProgressManager>();
        List<GameObject> npcs = progressManager.npcs;
        List<int> npcStates = progressManager.npcStates;

        for (int i = 0; i < npcs.Count; i++)
        {
            if (gameObject == npcs[i])
            {
                if (npcStates[i] == 0)
                {
                    Healed();
                }
            }
        }
    }

    public void TakeDamage()
    {
        hp -= damageTakenPerHit;

        if (hp <= 0)
        {
            Heal();
        }
    }

    public void Heal()
    {
        hp = 0;
        testCloud.SetActive(false);
        StartCoroutine("Reaction");
        gameObject.layer = LayerMask.NameToLayer("Obstacles");

        ProgressManager progressManager = GameObject.FindGameObjectsWithTag("Progress")[0].GetComponent<ProgressManager>();
        List<GameObject> npcs = progressManager.npcs;
        List<int> npcStates = progressManager.npcStates;

        for (int i = 0; i < npcs.Count; i++)
        {
            if (gameObject == npcs[i])
            {
                npcStates[i] = 0;
            }
        }
    }

    public void Healed()
    {
        hp = 0;
        testCloud.SetActive(false);
        gameObject.layer = LayerMask.NameToLayer("Obstacles");
    }

    IEnumerator Reaction()
    {
        yield return new WaitForSeconds(1f);
        reaction.SetActive(true);
        yield return new WaitForSeconds(3f);
        reaction.SetActive(false);
    }
}
