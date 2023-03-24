using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.SceneManagement;

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
    private float songPosRounded = 0f;

    public float beatDiffFix;
    public float rhythmThreshold;
    private float previousBeat;
    public bool musicPlaying = false;
    [HideInInspector]
    public bool gameIsPaused = false;

    public GameObject testIndicatorWhite;
    public SpriteRenderer symbolRenderer;
    public Sprite normalSymbol;
    public Sprite biggerSymbol;

    public GameObject lineHolder;
    public GameObject linePrefab;
    public float beatsShownInAdvance;
    private float lineInterval = 0.5f;
    [HideInInspector]
    public bool battleOn = false;
    private bool halfBeat = true;
    public SpriteRenderer lineBar;
    private Color lineBarOriginalColor;
    public Color lineBarBattleColor;

    // movement in this script is based on movepoint towards which the player will move
    public Transform movePoint;
    public Transform attackPoint;
    public float moveSpeed = 4f;
    public bool canMove = true;
    private int walkCombo = 0;
    public Transform bossRespawnPos;

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

    // create list of all npcs in the world and store them in progress manager
    private void Awake()
    {
        walkInstance = this;

        ProgressManager progressManager = GameObject.FindGameObjectsWithTag("Progress")[0].GetComponent<ProgressManager>();
        progressManager.CreateNPCList();

        if (progressManager.bossReached)
        {
            transform.position = bossRespawnPos.position;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        secPerBeat = 60f / songBpm;
        previousBeat = -1;
        hp100PosX = hpMask.transform.position.x;
        hp0PosX = hp100PosX - 1.15f;
        hpMax = hp;
        lineBarOriginalColor = lineBar.color;

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
        if (gameIsPaused)
        {
            return;
        }

        // BEAT

        songPosInSecs = (float)(AudioSettings.dspTime - songStartTime);
        songPosInBeats = songPosInSecs / secPerBeat;

        if (Physics.OverlapBox(transform.position, new Vector3(1.2f, 1.2f, 1.2f), Quaternion.identity, enemies).Length > 0)
        {
            battleOn = true;
            lineBar.color = lineBarBattleColor;
        }

        else
        {
            battleOn = false;
            lineBar.color = lineBarOriginalColor;
        }

        if ((songPosInBeats - previousBeat) >= (lineInterval + beatDiffFix) && musicPlaying)
        {
            GameObject newLineLeft = Instantiate(linePrefab, lineHolder.transform);
            newLineLeft.transform.position = new Vector3(-6f, -4f, 0f);

            Line lineLeft = newLineLeft.GetComponent<Line>();
            lineLeft.beatOfThisLine = previousBeat + beatsShownInAdvance + lineInterval + beatDiffFix;
            lineLeft.halfBeat = halfBeat;

            GameObject newLineRight = Instantiate(linePrefab, lineHolder.transform);
            newLineRight.transform.position = new Vector3(6f, -4f, 0f);

            Line lineRight = newLineRight.GetComponent<Line>();
            lineRight.beatOfThisLine = previousBeat + beatsShownInAdvance + lineInterval + beatDiffFix;
            lineRight.halfBeat = halfBeat;

            if ((!halfBeat || (halfBeat && battleOn)) && songPosInBeats > beatsShownInAdvance)
            {
                StartCoroutine("LineBarEffect");
            }

                previousBeat = previousBeat + lineInterval;
            halfBeat = !halfBeat;

            
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

                
                float oldSongPosRounded = songPosRounded;
                songPosRounded = Mathf.Round(songPosInBeats - beatDiffFix);

                if (songPosRounded - oldSongPosRounded > (1 + 2 * rhythmThreshold))
                {
                    walkCombo = 0;
                }

                // if songposinbeats is near whole number, player is on rhythm, if near half, player is not on rhythm
                float inaccuracy = Mathf.Abs((songPosInBeats - beatDiffFix) - songPosRounded);
                if (inaccuracy < rhythmThreshold)
                {
                    walkCombo++;

                    for (int i = 0; i < lights.Count; i++)
                    {
                        StartCoroutine("LightIntensify", lights[i]);
                    }

                    // only move if there are no obstacles or enemies
                    if (walkCombo > 4)
                    {
                        targetPos = transform.position + 2 * (targetPos - transform.position);

                        if (Physics.OverlapSphere(targetPos, .2f, obstacles).Length == 0 && Physics.OverlapSphere(targetPos, .2f, enemies).Length == 0)
                        {
                            movePoint.position = targetPos;
                        }
                    }
                }

                else
                {
                    walkCombo = 0;
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
                    songPosRounded = Mathf.Round(songPosInBeats - beatDiffFix);
                    float inaccuracy = Mathf.Abs((songPosInBeats - beatDiffFix) - songPosRounded);
                    if (inaccuracy < rhythmThreshold || inaccuracy > (0.5f - rhythmThreshold))
                    {
                        for (int i = 0; i < lights.Count; i++)
                        {
                            StartCoroutine("LightIntensify", lights[i]);
                        }

                        WorldNpc npc = enemy[0].gameObject.GetComponent<WorldNpc>();
                        npc.TakeDamage();
                    }

                    else
                    {
                        TakeDamage();
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
            Respawn();
        }

        float newPosX = Mathf.Lerp(hp100PosX, hp0PosX, (hpMax - hp) / hpMax);
        hpMask.transform.position = new Vector3(newPosX, hpMask.transform.position.y, hpMask.transform.position.z);
    }

    public void Respawn()
    {
        ProgressManager progressManager = GameObject.FindGameObjectsWithTag("Progress")[0].GetComponent<ProgressManager>();
        progressManager.resetNPCs = true;

        if (SceneManager.GetActiveScene().name == "WorldScene")
        {
            SceneManager.LoadScene("WorldScene");
        }

        else
        {
            SceneManager.LoadScene("XTESTWorld");
        }
    }

    IEnumerator StartMusic()
    {
        yield return new WaitForSeconds(1f);

        // record the time when the music starts
        songStartTime = (float)AudioSettings.dspTime;
        music.Play();
        musicPlaying = true;
    }

    IEnumerator LineBarEffect()
    {
        //testIndicatorWhite.SetActive(true);
        symbolRenderer.sprite = biggerSymbol;
        yield return new WaitForSeconds(0.12f);
        symbolRenderer.sprite = normalSymbol;
        //testIndicatorWhite.SetActive(false);
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
