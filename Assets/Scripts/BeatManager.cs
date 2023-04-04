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
    private float beat;

    public AudioSource music;
    public AudioSource deathMusic;
    public AudioSource damageSound;
    public GameObject fadeImage;
    public Animator fadeAnimator;
    public GameObject deathScreen;
    public GameObject deathText;
    public Animator deathTextAnimator;

    public float beatsShownInAdvance;
    private List<float> leftNotes = new List<float>();
    private List<float> rightNotes = new List<float>();
    private List<float> smashNotes = new List<float>();
    private List<float> timedFeedbacks = new List<float>();
    private List<string> timedFeedbackTexts = new List<string>();
    private int nextLeftIndex = 0;
    private int nextRightIndex = 0;
    private int nextSmashIndex = 0;
    private int nextFeedbackIndex = 0;

    public float noteSpawnPosX;
    public GameObject hitFlash;
    public GameObject leftNotePrefab;
    public GameObject rightNotePrefab;
    public GameObject smashNotePrefab;
    public ParticleSystem comboParticle;
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
        fadeImage.SetActive(true);

        if (GameObject.FindGameObjectsWithTag("Progress").Length == 0)
        {
            Instantiate(progressPrefab);
        }

        ProgressManager progressManager = GameObject.FindGameObjectsWithTag("Progress")[0].GetComponent<ProgressManager>();
        progressManager.previousScene = "BattleScene";
        progressManager.bossReached = true;

        CreateNotes();

        secPerBeat = 60f / songBpm;
        combo = 0;
        hp100PosX = hpMask.transform.position.x;
        hp0PosX = hp100PosX - 1.25f;
        hpMax = hp;
        progressBarStartPos = progressBar.transform.position;
        progressBarEndPos = new Vector3(0f, -4.86f, 0f);
        saturationMaterial.SetFloat("_SatValue", 0f);

        StartCoroutine("StartMusic");
    }

    // Update is called once per frame
    void Update()
    {
        if (gameIsPaused)
        {
            return;
        }

        // beat is based on dsptime which is more accurate
        // keep track of current beat
        songPosInSecs = (float)(AudioSettings.dspTime - songStartTime);
        songPosInBeats = songPosInSecs / secPerBeat;

        if (notesMove)
        {
            // spawn next note in advance so that it gets to the button on time
            if (nextLeftIndex < leftNotes.Count && leftNotes[nextLeftIndex] <= songPosInBeats + beatsShownInAdvance)
            {
                GameObject newNote = Instantiate(leftNotePrefab, noteHolder.transform);
                newNote.transform.position = new Vector3(noteSpawnPosX, 0f, 0f);

                Note note = newNote.GetComponent<Note>();
                note.beatOfThisNote = leftNotes[nextLeftIndex];

                nextLeftIndex++;
            }

            if (nextRightIndex < rightNotes.Count && rightNotes[nextRightIndex] <= songPosInBeats + beatsShownInAdvance)
            {
                GameObject newNote = Instantiate(rightNotePrefab, noteHolder.transform);
                newNote.transform.position = new Vector3(noteSpawnPosX, 0f, 0f);

                Note note = newNote.GetComponent<Note>();
                note.beatOfThisNote = rightNotes[nextRightIndex];

                nextRightIndex++;
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

            // start new timed feedback
            if (nextFeedbackIndex < timedFeedbacks.Count && timedFeedbacks[nextFeedbackIndex] <= songPosInBeats + beatsShownInAdvance)
            {
                feedbackText.color = new Color32(0, 255, 0, 255);
                feedbackText.text = timedFeedbackTexts[nextFeedbackIndex];

                nextFeedbackIndex++;
            }

            // interpolates song progress bar position based on current song position
            progressBar.transform.position = Vector3.Lerp(progressBarStartPos, progressBarEndPos, songPosInSecs / music.clip.length);

            float saturationValue = Mathf.Lerp(0f, 1f, songPosInSecs / music.clip.length);
            saturationMaterial.SetFloat("_SatValue", saturationValue);

            if (saturationValue == 1f)
            {
                StartCoroutine("EndScreen");
            }
        }
    }

    IEnumerator StartMusic()
    {
        yield return new WaitForSeconds(0.5f);

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
        feedbackText.color = new Color32(0, 255, 0, 255);
        feedbackText.text = "";
    }

    public void HideFeedback()
    {
        feedbackText.text = "";
    }

    public void TakeDamage(float damage)
    {
        damageSound.Play();
        hp -= damage;

        if(hp <= 0)
        {
            hp = 0;
            StartCoroutine("Death");
        }

        float newPosX = Mathf.Lerp(hp100PosX, hp0PosX, (hpMax - hp) / hpMax);
        hpMask.transform.position = new Vector3(newPosX, hpMask.transform.position.y, hpMask.transform.position.z);
    }

    IEnumerator Death()
    {
        gameIsPaused = true;
        fadeAnimator.SetTrigger("FadeOut");
        yield return new WaitForSeconds(1f);
        deathScreen.SetActive(true);
        fadeAnimator.SetTrigger("FadeIn");
        music.Stop();
        deathMusic.Play();
        yield return new WaitForSeconds(1f);
        deathText.SetActive(true);
        deathTextAnimator.SetTrigger("ShowText");
        yield return new WaitForSeconds(4f);
        deathText.SetActive(false);
        fadeAnimator.SetTrigger("FadeOut");
        yield return new WaitForSeconds(1f);

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
        yield return new WaitForSeconds(2f);
        fadeAnimator.SetTrigger("FadeOut");
        yield return new WaitForSeconds(1.5f);

        if (SceneManager.GetActiveScene().name == "BattleScene")
        {
            SceneManager.LoadScene("EndScene");
        }

        else
        {
            SceneManager.LoadScene("XTESTEnd");
        }
    }

    public void CreateNotes()
    {
        beat = beatsShownInAdvance + offsetInBeats;

        // NOTES

        // no notes in the beginning
        beat += 11;

        // 4 slow notes
        for (int i = 0; i < 2; i++)
        {
            leftNotes.Add(beat);
            beat += 4;
            rightNotes.Add(beat);
            beat += 4;
        }

        // basic melody
        for (int i = 0; i < 2; i++)
        {
            BasicMelody();
        }

        // simple part 1
        for (int i = 0; i < 4; i++)
        {
            rightNotes.Add(beat);
            beat++;
            leftNotes.Add(beat);
            beat++;
            rightNotes.Add(beat);
            beat += 2;
        }

        // simple part 2
        for (int i = 0; i < 4; i++)
        {
            leftNotes.Add(beat);
            beat++;
            rightNotes.Add(beat);
            beat++;
            leftNotes.Add(beat);
            beat += 2;
        }

        // strong notes
        for (int i = 0; i < 4; i++)
        {
            rightNotes.Add(beat);
            beat++;
        }
        beat = beat + 4;
        for (int i = 0; i < 4; i++)
        {
            leftNotes.Add(beat);
            beat++;
        }
        beat = beat + 4;

        // simple part 3
        for (int i = 0; i < 4; i++)
        {
            rightNotes.Add(beat);
            beat++;
            leftNotes.Add(beat);
            beat++;
            rightNotes.Add(beat);
            beat += 2;
        }

        // solo part
        SoloMelody();

        smashNotes.Add(beat);
        beat += 8;

        SoloMelody();

        smashNotes.Add(beat);
        beat += 8;

        // smash notes
        //beat += 8;
        //timedFeedbacks.Add(beat);
        //timedFeedbackTexts.Add("GET READY");
        //beat += 8;
        //timedFeedbacks.Add(beat);
        //timedFeedbackTexts.Add("TO HIT LEFT / RIGHT FAST");
        //beat += 8;
        //smashNotes.Add(beat);
        //beat += 16;
        //timedFeedbacks.Add(beat);
        //timedFeedbackTexts.Add("GET READY");
        //beat += 10;
        //timedFeedbacks.Add(beat);
        //timedFeedbackTexts.Add("TO HIT LEFT / RIGHT FAST");
        //beat += 6;
        //smashNotes.Add(beat);
        //beat += 8;

        // basic melody
        for (int i = 0; i < 2; i++)
        {
            BasicMelody();
        }

        // simple part 1
        for (int i = 0; i < 8; i++)
        {
            rightNotes.Add(beat);
            beat++;
            leftNotes.Add(beat);
            beat++;
            rightNotes.Add(beat);
            beat += 2;
        }

        // strong notes
        for (int i = 0; i < 4; i++)
        {
            rightNotes.Add(beat);
            beat++;
        }
        beat = beat + 4;
        for (int i = 0; i < 4; i++)
        {
            leftNotes.Add(beat);
            beat++;
        }
        beat = beat + 4;

        // simple part 2
        for (int i = 0; i < 4; i++)
        {
            rightNotes.Add(beat);
            beat++;
            leftNotes.Add(beat);
            beat++;
            rightNotes.Add(beat);
            beat += 2;
        }

        // end
        for (int i = 0; i < 2; i++)
        {
            leftNotes.Add(beat);
            beat += 2;
            rightNotes.Add(beat);
            beat += 2;
        }
    }

    void BasicMelody()
    {
        beat += 2;
        rightNotes.Add(beat);
        beat++;
        rightNotes.Add(beat);
        beat++;
        leftNotes.Add(beat);
        beat++;
        rightNotes.Add(beat);
        beat += 2;
        leftNotes.Add(beat);
        beat++;
        rightNotes.Add(beat);
        beat += 4;
        leftNotes.Add(beat);
        beat++;
        rightNotes.Add(beat);
        beat++;
        leftNotes.Add(beat);
        beat++;
        rightNotes.Add(beat);
        beat += 2;
        leftNotes.Add(beat);
        beat++;
        leftNotes.Add(beat);
        beat++;
        rightNotes.Add(beat);
        beat++;
        rightNotes.Add(beat);
        beat += 4;
        leftNotes.Add(beat);
        beat++;
        rightNotes.Add(beat);
        beat++;
        leftNotes.Add(beat);
        beat++;
        rightNotes.Add(beat);
        beat++;
        rightNotes.Add(beat);
        beat++;
        leftNotes.Add(beat);
        beat++;
        leftNotes.Add(beat);
        beat += 2;
    }

    void SoloMelody()
    {
        beat++;
        rightNotes.Add(beat);
        beat += 2;
        leftNotes.Add(beat);
        beat++;
        rightNotes.Add(beat);
        beat += 3;
        leftNotes.Add(beat);
        beat++;
        rightNotes.Add(beat);
        beat += 2;
        leftNotes.Add(beat);
        beat += 2;
        leftNotes.Add(beat);
        beat++;
        rightNotes.Add(beat);
        beat++;
        leftNotes.Add(beat);
        beat++;
        rightNotes.Add(beat);
        beat += 2;
        leftNotes.Add(beat);
        beat += 2;
        rightNotes.Add(beat);
        beat++;
        leftNotes.Add(beat);
        beat += 3;
        rightNotes.Add(beat);
        beat++;
    }
}
