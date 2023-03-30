using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class BeatManager : MonoBehaviour
{
    public static BeatManager beatInstance;
    public GameObject progressPrefab;

    public float songBpm;
    private float secPerBeat;
    private float songPosInSecs;
    [HideInInspector]
    public float songPosInBeats;
    private float songStartTime;

    public AudioSource music;
    public AudioSource damageSound;

    public float beatsShownInAdvance;
    private List<float> notes = new List<float>();
    private List<float> smashNotes = new List<float>();
    private int nextIndex = 0;
    private int nextSmashIndex = 0;
    private int colorIndex;

    public float noteSpawnPosX;
    public GameObject hitFlash;
    public GameObject notePrefab1;
    public GameObject notePrefab2;
    public ParticleSystem comboParticle;
    private GameObject[] colors = new GameObject[2];
    public GameObject smashNotePrefab;
    public float smashHitsNeeded = 50f;

    [HideInInspector]
    public bool gameIsPaused = false;
    private bool notesMove = false;
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
    public int pointsPerSmashHit = 50;

    private int goodHits = 0;
    private int greatHits = 0;
    private int perfectHits = 0;
    private int smashHits = 0;
    private int misses = 0;

    public GameObject hpMask;
    private float hp0PosX;
    private float hp100PosX;
    private float hp = 100f;
    private float hpMax;
    public float damagePerMiss = 10f;
    public float damagePerFail = 30f;

    public GameObject progressBar;
    private Vector3 progressBarStartPos;
    private Vector3 progressBarEndPos;
    public Material saturationMaterial;

    // Start is called before the first frame update
    void Start()
    {
        beatInstance = this;
        Cursor.visible = false;

        if (GameObject.FindGameObjectsWithTag("Progress").Length == 0)
        {
            Instantiate(progressPrefab);
        }

        ProgressManager progressManager = GameObject.FindGameObjectsWithTag("Progress")[0].GetComponent<ProgressManager>();
        progressManager.previousScene = "BattleScene";

        CreateNotes();

        colors[0] = notePrefab1; colors[1] = notePrefab2;
        secPerBeat = 60f / songBpm;
        combo = 0;
        hp100PosX = hpMask.transform.position.x;
        hp0PosX = hp100PosX - 1.15f;
        hpMax = hp;
        progressBarStartPos = progressBar.transform.position;
        progressBarEndPos = new Vector3(0f, -4.86f, 0f);

        StartCoroutine("StartMusic");
    }

    // Update is called once per frame
    void Update()
    {
        if (gameIsPaused)
        {
            return;
        }

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

            // start new smashnote
            if (nextSmashIndex < smashNotes.Count && smashNotes[nextSmashIndex] <= songPosInBeats + beatsShownInAdvance)
            {
                GameObject newSmashNote = Instantiate(smashNotePrefab, noteHolder.transform);
                newSmashNote.transform.position = new Vector3(noteSpawnPosX, 0f, 0f);

                SmashNote smashNote = newSmashNote.GetComponent<SmashNote>();
                smashNote.beatOfThisNote = smashNotes[nextSmashIndex];
                smashNote.hitsNeeded = smashHitsNeeded;

                nextSmashIndex++;
            }

            // interpolates song progress bar position based on current song position
            progressBar.transform.position = Vector3.Lerp(progressBarStartPos, progressBarEndPos, songPosInSecs / music.clip.length);

            float saturationValue = Mathf.Lerp(0f, 1f, songPosInSecs / music.clip.length);
            saturationMaterial.SetFloat("_SatValue", saturationValue);
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

        TakeDamage(damagePerMiss);
    }

    public void SmashHit()
    {
        points += pointsPerSmashHit;
        smashHits++;
        StartCoroutine("SmashFlash");

        pointsText.text = "" + points.ToString();
    }

    public void SmashFail()
    {
        combo = 0;
        misses++;

        feedbackText.color = new Color(255, 0, 0);
        feedbackText.text = "Fail";
        comboText.text = "" + combo;

        TakeDamage(damagePerFail);
    }

    IEnumerator Flash()
    {
        if(combo % 10f == 0f && combo != 0)
        {
            comboParticle.Play();
        }

        hitFlash.SetActive(true);
        yield return new WaitForSeconds(0.05f);
        hitFlash.SetActive(false);
    }

    IEnumerator SmashFlash()
    {
        comboParticle.Play();

        hitFlash.SetActive(true);
        yield return new WaitForSeconds(0.05f);
        hitFlash.SetActive(false);
    }

    public void ComboParticle()
    {
        comboParticle.Play();
    }

    public void SmashFeedback()
    {
        pointsText.text = "" + points.ToString();
        feedbackText.color = new Color32(253, 215, 148, 255);
        feedbackText.text = "SMASH";
    }

    public void HideFeedback()
    {
        feedbackText.text = "";
    }

    public void CreateNotes()
    {
        float beat = beatsShownInAdvance + offsetInBeats;

        for (int i = 0; i < 11; i++)
        {
            int noteOrNot = Random.Range(0, 4);

            if (noteOrNot != 0)
            {
                notes.Add(beat);
            }
            
            beat++;
        }

        beat++;
        smashNotes.Add(beat);
        beat = beat + 9;

        for (int i = 0; i < 14; i++)
        {
            int noteOrNot = Random.Range(0, 4);

            if (noteOrNot != 0)
            {
                notes.Add(beat);
            }

            beat++;
        }

        beat++;
        smashNotes.Add(beat);
        beat = beat + 9;

        for (int i = 0; i < 14; i++)
        {
            int noteOrNot = Random.Range(0, 4);

            if (noteOrNot != 0)
            {
                notes.Add(beat);
            }

            beat++;
        }
    }

    public void TakeDamage(float damage)
    {
        damageSound.Play();
        hp -= damage;

        if(hp <= 0)
        {
            hp = 0;
            Respawn();
        }

        float newPosX = Mathf.Lerp(hp100PosX, hp0PosX, (hpMax - hp) / hpMax);
        hpMask.transform.position = new Vector3(newPosX, hpMask.transform.position.y, hpMask.transform.position.z);
    }

    public void Respawn()
    {
        if (SceneManager.GetActiveScene().name == "BattleScene")
        {
            SceneManager.LoadScene("WorldScene");
        }

        else
        {
            SceneManager.LoadScene("XTESTWorld");
        }
    }

    IEnumerator EndScreen()
    {
        yield return new WaitForSeconds(3f);

        if (SceneManager.GetActiveScene().name == "BattleScene")
        {
            SceneManager.LoadScene("EndScene");
        }

        else
        {
            SceneManager.LoadScene("XTESTEnd");
        }
    }
}
