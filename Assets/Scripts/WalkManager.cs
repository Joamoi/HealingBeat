using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RendererUtils;

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
    private float lineInterval = 1f;

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

    public Volume volume;
    private Bloom bloom;
    private ColorAdjustments colorAdj;
    private List<Light> lights = new List<Light>();
    public float lightMultiplier;
    public float bloomThreshold;
    public float postExposure;
    public float lightDuration = 0.15f;

    // Start is called before the first frame update
    void Start()
    {
        walkInstance = this;

        secPerBeat = 60f / songBpm;
        previousBeat = -1;
        hp100PosX = hpMask.transform.position.x;
        hp0PosX = hp100PosX - 1.15f;
        hpMax = hp;

        GameObject[] lightObjects = GameObject.FindGameObjectsWithTag("Light");
        
        for (int i = 0; i < lightObjects.Length; i++)
        {
            lights.Add(lightObjects[i].GetComponent<Light>());
        }

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

        if (Physics.OverlapBox(transform.position, new Vector3(1.2f, 1.2f, 1.2f), Quaternion.identity, enemies).Length > 0)
        {
            if (lineInterval == 1f)
            {
                lineInterval = 0.5f;
            }
        }

        else
        {
            lineInterval = 1f;
        }

        if ((songPosInBeats - previousBeat) > (lineInterval + beatDiffFix) && musicPlaying)
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

                    for (int i = 0; i < lights.Count; i++)
                    {
                        StartCoroutine("LightIntensify", lights[i]);
                    }
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

                    // if songposinbeats is near half number, player is on rhythm, if not, player is not on rhythm
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

    IEnumerator LightIntensify(Light light)
    {
        volume.profile.TryGet<Bloom>(out bloom);
        volume.profile.TryGet<ColorAdjustments>(out colorAdj);

        float originalBloomThresh = bloom.threshold.value;
        float originalPostExpo = colorAdj.postExposure.value;
        float originalIntensity = light.intensity;

        bloom.threshold.value -= 0.5f * bloomThreshold;
        colorAdj.postExposure.value += 0.5f * postExposure;
        light.intensity += 0.5f * lightMultiplier * originalIntensity;

        yield return new WaitForSeconds(lightDuration / 3f);

        bloom.threshold.value -= 0.5f * bloomThreshold;
        colorAdj.postExposure.value += 0.5f * postExposure;
        light.intensity += 0.5f * lightMultiplier * originalIntensity;

        yield return new WaitForSeconds(lightDuration / 3f);

        bloom.threshold.value += 0.5f * bloomThreshold;
        colorAdj.postExposure.value -= 0.5f * postExposure;
        light.intensity -= 0.5f * lightMultiplier * originalIntensity;

        yield return new WaitForSeconds(lightDuration / 3f);

        bloom.threshold.value += 0.5f * bloomThreshold;
        colorAdj.postExposure.value -= 0.5f * postExposure;
        light.intensity -= 0.5f * lightMultiplier * originalIntensity;

    }
}
