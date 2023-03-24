using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour
{
    private Vector3 spawnPos;
    private Vector3 removePos;
    private Vector3 dir;
    private float beatsShownInAdvance;
    [HideInInspector]
    public float beatOfThisLine;
    [HideInInspector]
    public bool halfBeat = false;

    // Start is called before the first frame update
    void Start()
    {
        beatsShownInAdvance = WalkManager.walkInstance.beatsShownInAdvance;
        spawnPos = transform.position;
        dir = new Vector3(0f, -4f, 0f) - transform.position;
        removePos = new Vector3(0f, -4f, 0f) - 0.03f * dir;
    }

    // Update is called once per frame
    void Update()
    {
        // interpolates line position based on current song position, beat of the note and amount of beats shown in advance
        transform.position = Vector3.Lerp(spawnPos, removePos, (beatsShownInAdvance - (beatOfThisLine - WalkManager.walkInstance.songPosInBeats)) / beatsShownInAdvance);

        if (transform.position == removePos)
        {
            Destroy(gameObject);
        }

        if (halfBeat)
        {
            gameObject.GetComponent<SpriteRenderer>().enabled = WalkManager.walkInstance.battleOn;
        }
    }
}
