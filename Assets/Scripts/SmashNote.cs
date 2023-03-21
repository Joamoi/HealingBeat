using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmashNote : MonoBehaviour
{
    public bool canBePressed;
    public KeyCode keyToPress1;
    public KeyCode keyToPress2;
    public KeyCode keyToPress3;
    public KeyCode keyToPress4;
    public KeyCode keyToPress5;
    public KeyCode keyToPress6;
    public KeyCode keyToPress7;
    public KeyCode keyToPress8;

    public float travelDistance;
    public float buttonPosX;

    private Vector3 spawnPos;
    private Vector3 removePos;
    private Vector3 originalRemovePos;
    private float beatsShownInAdvance;
    [HideInInspector]
    public float beatOfThisNote;

    public GameObject noteEnd;
    public SpriteRenderer lineSprite;
    [HideInInspector]
    public float hitsNeeded;
    private float hitsDone = 0f;
    public Color smashPassColor;
    private bool hitCheckDone = false;

    // Start is called before the first frame update
    void Start()
    {
        beatsShownInAdvance = BeatManager.beatInstance.beatsShownInAdvance;
        spawnPos = transform.position;
        removePos = transform.position + new Vector3(-travelDistance, 0f, 0f);
        originalRemovePos = removePos;
    }

    // Update is called once per frame
    void Update()
    {
        // interpolates note position based on current song position, beat of the note and amount of beats shown in advance
        transform.position = Vector3.Lerp(spawnPos, removePos, (beatsShownInAdvance - (beatOfThisNote - BeatManager.beatInstance.songPosInBeats)) / beatsShownInAdvance);

        if (transform.position == removePos)
        {
            spawnPos = removePos;
            removePos = transform.position + new Vector3(-travelDistance, 0f, 0f);
            beatOfThisNote += beatsShownInAdvance;
        }

        // could be simpler, getAxis doesn't work if we need both buttons at the same time
        if (canBePressed && (Input.GetKeyDown(keyToPress1) || Input.GetKeyDown(keyToPress2) || Input.GetKeyDown(keyToPress3) || Input.GetKeyDown(keyToPress4) || Input.GetKeyDown(keyToPress5) || Input.GetKeyDown(keyToPress6) || Input.GetKeyDown(keyToPress7) || Input.GetKeyDown(keyToPress8)))
        {
            BeatManager.beatInstance.SmashHit();
            hitsDone++;
            Debug.Log(hitsDone);

        }

        if (hitsDone >= hitsNeeded)
        {
            lineSprite.color = smashPassColor;
        }

        //float green = 255f - (Mathf.Clamp(hitsDone, 0f, hitsNeeded) / hitsNeeded) * 255f;
        //float blue = (Mathf.Clamp(hitsDone, 0f, hitsNeeded) / hitsNeeded) * 255f;
        //lineSprite.color = new Color(0f, green, blue, 150f);

        if (noteEnd.transform.position.x - (buttonPosX - 0.6f) <= 0 && !hitCheckDone)
        {
            hitCheckDone = true;

            if (hitsDone < hitsNeeded)
            {
                BeatManager.beatInstance.SmashFail();
            }
        }

        if (noteEnd.transform.position.x - originalRemovePos.x <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Button")
        {
            canBePressed = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Button")
        {
            canBePressed = false;
        }
    }
}