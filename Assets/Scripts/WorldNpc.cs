using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldNpc : MonoBehaviour
{
    public float hp = 5f;
    public float healPerHit = 12f;
    private float hpFullPosX;
    private float hpStartPosX;
    private float hpMax;
    public ParticleSystem cloud;
    public GameObject reaction;
    public GameObject hpMask;
    public GameObject hpObject;
    public Animator animator;
    public ParticleSystem healSparkle;

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

        hpStartPosX = hpMask.transform.position.x;
        hpFullPosX = hpStartPosX + 1.01f;
        hpMax = 100f;

        float newPosX = Mathf.Lerp(hpStartPosX, hpFullPosX, hp / hpMax);
        hpMask.transform.position = new Vector3(newPosX, hpMask.transform.position.y, hpMask.transform.position.z);
    }

    public void TakeHeal()
    {
        hp += healPerHit;

        if (hp >= 100)
        {
            Heal();
        }

        float newPosX = Mathf.Lerp(hpStartPosX, hpFullPosX, hp / hpMax);
        hpMask.transform.position = new Vector3(newPosX, hpMask.transform.position.y, hpMask.transform.position.z);
    }

    public void Heal()
    {
        hp = 100;
        cloud.Stop();
        StartCoroutine("Reaction");
        gameObject.layer = LayerMask.NameToLayer("Obstacles");

        ProgressManager progressManager = GameObject.FindGameObjectsWithTag("Progress")[0].GetComponent<ProgressManager>();
        progressManager.npcsLeft--;
        List<GameObject> npcs = progressManager.npcs;
        List<int> npcStates = progressManager.npcStates;

        for (int i = 0; i < npcs.Count; i++)
        {
            if (gameObject == npcs[i])
            {
                npcStates[i] = 0;
            }
        }

        WalkManager.walkInstance.BattleOver();
        WalkManager.walkInstance.HealSound();
        healSparkle.Play();

        if (progressManager.npcsLeft == 0)
        {
            WalkManager.walkInstance.bossWall.SetActive(false);
        }
    }

    public void Healed()
    {
        hp = 100;
        cloud.Stop();
        animator.SetTrigger("Healed");
        gameObject.layer = LayerMask.NameToLayer("Obstacles");
    }

    IEnumerator Reaction()
    {
        yield return new WaitForSeconds(1f);
        animator.SetTrigger("Healed");
        hpObject.SetActive(false);
        reaction.SetActive(true);
        yield return new WaitForSeconds(3f);
        reaction.SetActive(false);
    }
}
