using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BeatManager : MonoBehaviour
{
    public static BeatManager beatInstance;

    public float songBpm;
    private float secPerBeat;
    private float songPosInSecs;
    [HideInInspector]
    public float songPosInBeats;
    private float songStartTime;

    public float beatsShownInAdvance;
    private List<float> notes = new List<float>();
    private int nextIndex = 0;
    private int colorIndex;

    public float noteSpawnPosX;
    public GameObject hitFlash;
    public GameObject notePrefab1;
    public GameObject notePrefab2;
    private GameObject[] colors = new GameObject[2];

    private bool notesMove = false;
    public AudioSource music;
    public GameObject noteHolder;
    public float offsetInBeats;

    public TextMeshProUGUI comboText;
    public TextMeshProUGUI pointsText;
    public TextMeshProUGUI feedbackText;

    private int combo;
    private int multiplier;
    private int points;
    public int pointsPerGoodHit = 100;
    public int pointsPerGreatHit = 200;
    public int pointsPerPerfectHit = 300;

    private int goodHits = 0;
    private int greatHits = 0;
    private int perfectHits = 0;
    private int misses = 0;

    public GameObject hpMask;
    private float hp0PosX;
    private float hp100PosX;
    private float hp = 100f;
    private float hpMax;
    public float damagePerMiss;

    // Start is called before the first frame update
    void Start()
    {
        beatInstance = this;

        CreateNotes();

        colors[0] = notePrefab1; colors[1] = notePrefab2;
        secPerBeat = 60f / songBpm;
        combo = 0;
        hp100PosX = hpMask.transform.position.x;
        hp0PosX = hp100PosX - 1.15f;
        hpMax = hp;

        StartCoroutine("StartMusic");
    }

    // Update is called once per frame
    void Update()
    {
        // keep track of current beat
        songPosInSecs = (float)(AudioSettings.dspTime - songStartTime);
        songPosInBeats = songPosInSecs / secPerBeat;

        if (notesMove)
        {
            // spawn next note in advance so that it gets to the button on time
            if (nextIndex < notes.Count && notes[nextIndex] <= songPosInBeats + beatsShownInAdvance)
            {
                colorIndex = Random.Range(0, 2);

                GameObject newNote = Instantiate(colors[colorIndex], noteHolder.transform);
                newNote.transform.position = new Vector3(noteSpawnPosX, 0f, 0f);

                Note note = newNote.GetComponent<Note>();
                note.beatOfThisNote = notes[nextIndex];

                nextIndex++;
            }
        }
    }

    IEnumerator StartMusic()
    {
        yield return new WaitForSeconds(1f);

        // record the time when the music starts
        songStartTime = (float)AudioSettings.dspTime;
        music.Play();

        // wait a manually checked amount of time before notes start to spawn
        yield return new WaitForSeconds(offsetInBeats * secPerBeat);
        notesMove = true;
    }

    public void PerfectHit()
    {
        combo++;
        multiplier = Mathf.Clamp(combo, 10, 50) / 10;
        points += multiplier * pointsPerPerfectHit;
        perfectHits++;
        StartCoroutine("Flash");

        pointsText.text = "" + points.ToString();
        feedbackText.color = new Color(255, 0, 255);
        feedbackText.text = "Perfect";
        comboText.text = "" + combo;
    }

    public void GreatHit()
    {
        combo++;
        multiplier = Mathf.Clamp(combo, 10, 50) / 10;
        points += multiplier * pointsPerGreatHit;
        greatHits++;
        StartCoroutine("Flash");

        pointsText.text = "" + points.ToString();
        feedbackText.color = new Color(0, 0, 255);
        feedbackText.text = "Great";
        comboText.text = "" + combo;
    }

    public void GoodHit()
    {
        combo++;
        multiplier = Mathf.Clamp(combo, 10, 50) / 10;
        points += multiplier * pointsPerGoodHit;
        goodHits++;
        StartCoroutine("Flash");

        pointsText.text = "" + points.ToString();
        feedbackText.color = new Color(255, 255, 0);
        feedbackText.text = "Good";
        comboText.text = "" + combo;
    }

    public void Miss()
    {
        combo = 0;
        misses++;

        feedbackText.color = new Color(255, 0, 0);
        feedbackText.text = "Miss";
        comboText.text = "" + combo;

        TakeDamage();
    }

    IEnumerator Flash()
    {
        hitFlash.SetActive(true);
        yield return new WaitForSeconds(0.05f);
        hitFlash.SetActive(false);
    }

    public void CreateNotes()
    {
        float beat = beatsShownInAdvance + offsetInBeats;

        for (int i = 0; i < 50; i++)
        {
            notes.Add(beat);
            beat = beat + 2f;
        }
    }

    public void TakeDamage()
    {
        hp -= damagePerMiss;

        if(hp <= 0)
        {
            hp = 0;
        }

        float newPosX = Mathf.Lerp(hp100PosX, hp0PosX, (hpMax - hp) / hpMax);
        hpMask.transform.position = new Vector3(newPosX, hpMask.transform.position.y, hpMask.transform.position.z);
    }
}
