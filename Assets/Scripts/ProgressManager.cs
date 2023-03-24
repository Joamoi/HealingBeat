using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProgressManager : MonoBehaviour
{
    // keeps track of progress like npc heals

    [HideInInspector]
    public bool bossReached = false;
    [HideInInspector]
    public List<GameObject> npcs = new List<GameObject>();
    [HideInInspector]
    public List<int> npcStates = new List<int>();
    [HideInInspector]
    public bool resetNPCs = true;

    // this object isn't destroyed between scenes
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void CreateNPCList()
    {
        GameObject npcHolder = GameObject.FindGameObjectWithTag("NPCHolder");

        // save the depressed state of all npcs to a list, done only the first time player enters world scene or if player dies in world scene
        if (resetNPCs)
        {
            npcStates.Clear();

            foreach (Transform npcTransform in npcHolder.transform)
            {
                npcStates.Add(1);
            }

            resetNPCs = false;
        }

        // gather list of npcs every time because when scene is loaded all npcs are "new"

        npcs.Clear();

        foreach (Transform npcTransform in npcHolder.transform)
        {
            npcs.Add(npcTransform.gameObject);
        }
    }
}
