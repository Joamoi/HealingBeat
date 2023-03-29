using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    public bool canBePressed;
    public KeyCode keyToPress1;
    public KeyCode keyToPress2;

    public float travelDistance;
    public float buttonPosX;

    private Vector3 spawnPos;
    private Vector3 removePos;
    private float beatsShownInAdvance;
    [HideInInspector]
    public float beatOfThisNote;

    // Start is called before the first frame update
    void Start()
    {
        beatsShownInAdvance = BeatManager.beatInstance.beatsShownInAdvance;
        spawnPos = transform.position;
        removePos = transform.position + new Vector3(-travelDistance, 0f, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        // interpolates note position based on current song position, beat of the note and amount of beats shown in advance
        transform.position = Vector3.Lerp(spawnPos, removePos, (beatsShownInAdvance - (beatOfThisNote - BeatManager.beatInstance.songPosInBeats)) / beatsShownInAdvance);

        // different distances to button give different accuracy
        if (canBePressed && (Input.GetKeyDown(keyToPress1) || Input.GetKeyDown(keyToPress2)))
        {
            if (Mathf.Abs(buttonPosX - transform.position.x) > 0.25f)
            {
                BeatManager.beatInstance.GoodHit();
                Destroy(gameObject);
            }

            else if (Mathf.Abs(buttonPosX - transform.position.x) < 0.1f)
            {
                BeatManager.beatInstance.PerfectHit();
                Destroy(gameObject);
            }

            else
            {
                BeatManager.beatInstance.GreatHit();
                Destroy(gameObject);
            }
        }

        if (transform.position == removePos)
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

        if (collision.tag == "Miss")
        {
            BeatManager.beatInstance.Miss();
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
