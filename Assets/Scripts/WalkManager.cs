using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkManager : MonoBehaviour
{
    public static WalkManager walkInstance;

    public float songBpm;
    private float secPerBeat;
    private float songPosInSecs;
    [HideInInspector]
    public float songPosInBeats;
    private float songStartTime;
    public AudioSource music;
    private int nextBeat;

    public float beatDiffFix;
    public float pressDiffFix;
    public float rhythmThreshold;
    private float previousBeat;
    public bool musicPlaying = false;

    public GameObject testIndicatorWhite;
    public GameObject testIndicatorGreen;
    public GameObject testIndicatorRed;

    public GameObject lineHolder;
    public GameObject linePrefab;
    public float beatsShownInAdvance;

    // movement in this script is based on movepoint towards which the player will move
    public Transform movePoint;
    public Transform attackPoint;
    public float moveSpeed = 4f;
    public bool canMove = true;

    public LayerMask obstacles;
    public LayerMask enemies;

    public GameObject hpMask;
    private float hp0PosX;
    private float hp100PosX;
    private float hp = 100f;
    private float hpMax;
    public float damagePerMiss;

    // Start is called before the first frame update
    void Start()
    {
        walkInstance = this;

        secPerBeat = 60f / songBpm;
        previousBeat = -1;
        hp100PosX = hpMask.transform.position.x;
        hp0PosX = hp100PosX - 1.15f;
        hpMax = hp;

        StartCoroutine("StartMusic");

        // remove parenting from movepoint so that it can move independently
        movePoint.parent = null;
    }

    // Update is called once per frame
    void Update()
    {
        // BEAT

        songPosInSecs = (float)(AudioSettings.dspTime - songStartTime);
        songPosInBeats = songPosInSecs / secPerBeat;

        if ((songPosInBeats - previousBeat) > (1 + beatDiffFix) && musicPlaying)
        {
            GameObject newLineLeft = Instantiate(linePrefab, lineHolder.transform);
            newLineLeft.transform.position = new Vector3(-6f, -4f, 0f);

            Line lineLeft = newLineLeft.GetComponent<Line>();
            lineLeft.beatOfThisLine = previousBeat + beatsShownInAdvance + 1 + beatDiffFix;

            GameObject newLineRight = Instantiate(linePrefab, lineHolder.transform);
            newLineRight.transform.position = new Vector3(6f, -4f, 0f);

            Line lineRight = newLineRight.GetComponent<Line>();
            lineRight.beatOfThisLine = previousBeat + beatsShownInAdvance + 1 + beatDiffFix;

            previousBeat++;

            if (previousBeat >= beatsShownInAdvance)
            {
                StartCoroutine("TestIndicatorWhite");
            }
        } 

        // MOVE

        // move player towards movepoint every frame
        transform.position = Vector3.MoveTowards(transform.position, movePoint.position, moveSpeed * Time.deltaTime);

        float moveHori = Input.GetAxisRaw("Horizontal");
        float moveVert = Input.GetAxisRaw("Vertical");

        // check hori and vert movement separately to disable diagonal movement
        if (Mathf.Abs(moveHori) == 1f && canMove)
        {
            Vector3 targetPos = transform.position + new Vector3(moveHori, 0f, 0f);
            TryToMove(targetPos);
        }
        else if (Mathf.Abs(moveVert) == 1f && canMove)
        {
            Vector3 targetPos = transform.position + new Vector3(0f, 0f, moveVert);
            TryToMove(targetPos);
        }

        // re-enable movement after releasing movement buttons
        if(moveHori == 0f && moveVert == 0f)
        {
            canMove = true;
        }
    }

    public void TryToMove(Vector3 targetPos)
    {
        // only move if player has reached the movepoint
        if (Vector3.Distance(transform.position, movePoint.position) == 0f)
        {
            // only move if there are no obstacles or enemies
            if (Physics.OverlapSphere(targetPos, .2f, obstacles).Length == 0 && Physics.OverlapSphere(targetPos, .2f, enemies).Length == 0)
            {
                movePoint.position = targetPos;
                canMove = false;

                // if songposinbeats is near whole number, player is on rhythm, if near half, player is not on rhythm
                float songPosRounded = Mathf.Round(songPosInBeats + pressDiffFix);
                if (Mathf.Abs((songPosInBeats + pressDiffFix) - songPosRounded) < rhythmThreshold)
                {
                    StartCoroutine("TestIndicatorGreen");
                }

                else
                {
                    StartCoroutine("TestIndicatorRed");
                }
            }

            else
            {
                Collider[] enemy = Physics.OverlapSphere(targetPos, .2f, enemies);

                // attack if there is an enemy
                if (enemy.Length > 0)
                {
                    attackPoint.position = targetPos;
                    canMove = false;

                    // if songposinbeats is near whole number, player is on rhythm, if near half, player is not on rhythm
                    float songPosRounded = Mathf.Round(songPosInBeats + pressDiffFix);
                    if (Mathf.Abs((songPosInBeats + pressDiffFix) - songPosRounded) < rhythmThreshold)
                    {
                        WorldNpc npc = enemy[0].gameObject.GetComponent<WorldNpc>();
                        npc.TakeDamage();

                        StartCoroutine("TestIndicatorGreen");
                    }

                    else
                    {
                        TakeDamage();

                        StartCoroutine("TestIndicatorRed");
                    }
                }
            }
        }
    }

    public void TakeDamage()
    {
        hp -= damagePerMiss;

        if (hp <= 0)
        {
            hp = 0;
        }

        float newPosX = Mathf.Lerp(hp100PosX, hp0PosX, (hpMax - hp) / hpMax);
        hpMask.transform.position = new Vector3(newPosX, hpMask.transform.position.y, hpMask.transform.position.z);
    }

    IEnumerator StartMusic()
    {
        yield return new WaitForSeconds(1f);

        // record the time when the music starts
        songStartTime = (float)AudioSettings.dspTime;
        music.Play();
        musicPlaying = true;
    }

    IEnumerator TestIndicatorWhite()
    {
        testIndicatorWhite.SetActive(true);
        yield return new WaitForSeconds(0.01f);
        testIndicatorWhite.SetActive(false);
    }

    IEnumerator TestIndicatorGreen()
    {
        testIndicatorGreen.SetActive(true);
        yield return new WaitForSeconds(0.01f);
        testIndicatorGreen.SetActive(false);
    }

    IEnumerator TestIndicatorRed()
    {
        testIndicatorRed.SetActive(true);
        yield return new WaitForSeconds(0.01f);
        testIndicatorRed.SetActive(false);
    }
}
