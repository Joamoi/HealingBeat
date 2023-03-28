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
    public GameObject progressPrefab;

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
    [HideInInspector]
    public bool musicPlaying = false;
    [HideInInspector]
    public bool gameIsPaused = false;

    public GameObject lineHolder;
    public GameObject linePrefab;
    public float beatsShownInAdvance;
    private float lineInterval = 1f;
    [HideInInspector]
    public bool battleOn = false;
    public SpriteRenderer lineBar;
    private Color lineBarOriginalColor;
    public Color lineBarBattleColor;
    public Animator symbolAnimator;

    public Transform playerModel;
    public Animator animator;
    public Transform movePoint;
    public Transform attackPoint;
    public float moveSpeed = 4f;
    public bool canMove = true;
    private int walkCombo = 0;
    public Transform jumpParticleParent;
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

    public SpriteRenderer leftBunny;
    public SpriteRenderer rightBunny;
    public Animator leftBunnyAnimator;
    public Animator rightBunnyAnimator;
    private SpriteRenderer[] bunnies = new SpriteRenderer[2];
    private float[] bunnyValues = new float[2];
    private float bunnyValue = 0f;
    private GameObject npcObject;
    private bool canHit = false;

    // create list of all npcs in the world and store them in progress manager
    private void Awake()
    {
        walkInstance = this;

        if (GameObject.FindGameObjectsWithTag("Progress").Length == 0)
        {
            Instantiate(progressPrefab);
        }

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
        bunnies[0] = leftBunny; bunnies[1] = rightBunny;
        bunnyValues[0] = -1; bunnyValues[1] = 1;

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

        if ((songPosInBeats - previousBeat) >= (lineInterval + beatDiffFix) && musicPlaying)
        {
            GameObject newLineLeft = Instantiate(linePrefab, lineHolder.transform);
            newLineLeft.transform.position = new Vector3(-6f, -4f, 0f);

            Line lineLeft = newLineLeft.GetComponent<Line>();
            lineLeft.beatOfThisLine = previousBeat + beatsShownInAdvance + lineInterval + beatDiffFix;

            GameObject newLineRight = Instantiate(linePrefab, lineHolder.transform);
            newLineRight.transform.position = new Vector3(6f, -4f, 0f);

            Line lineRight = newLineRight.GetComponent<Line>();
            lineRight.beatOfThisLine = previousBeat + beatsShownInAdvance + lineInterval + beatDiffFix;

            previousBeat = previousBeat + lineInterval;

            if (songPosInBeats > beatsShownInAdvance)
            {
                symbolAnimator.SetTrigger("Pulse");
                leftBunnyAnimator.SetTrigger("Pulse");
                rightBunnyAnimator.SetTrigger("Pulse");
            }
        }

        // MOVE

        // movement in this script is based on movepoint towards which the player will move
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
        if(moveHori == 0f && moveVert == 0f && !battleOn)
        {
            canMove = true;
        }

        // NPC BATTLE

        if (battleOn && canHit)
        {
            if (moveHori != 0)
            {
                CheckBattleHit(moveHori);
            }
        }

        // re-enable movement after releasing movement buttons
        if (moveHori == 0f && moveVert == 0f && battleOn)
        {
            canHit = true;
        }
    }

    public void TryToMove(Vector3 targetPos)
    {
        // turn the player to face the direction it's trying to move
        Vector3 faceDir = targetPos - transform.position;
        float angle = Vector3.SignedAngle(playerModel.transform.forward, faceDir, Vector3.up);
        playerModel.transform.RotateAround(transform.position, Vector3.up, angle);

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

                // moving combo fails if player doesn't move every beat
                if (songPosRounded - oldSongPosRounded > (1 + 2 * rhythmThreshold))
                {
                    walkCombo = 0;
                }

                // if songposinbeats is near whole number, player is on rhythm, if near half, player is not on rhythm
                float inaccuracy = Mathf.Abs((songPosInBeats - beatDiffFix) - songPosRounded);
                if (inaccuracy < rhythmThreshold)
                {
                    walkCombo++;
                    
                    foreach (Transform particle in jumpParticleParent)
                    {
                        particle.GetComponent<ParticleSystem>().Play();
                    }

                    for (int i = 0; i < lights.Count; i++)
                    {
                        StartCoroutine("LightIntensify", lights[i]);
                    }

                    // only move if there are no obstacles or enemies
                    // move 2 tiles with combo, 1 otherwise
                    if (walkCombo > 4)
                    {
                        targetPos = transform.position + 2 * (targetPos - transform.position);

                        if (Physics.OverlapSphere(targetPos, .2f, obstacles).Length == 0 && Physics.OverlapSphere(targetPos, .2f, enemies).Length == 0)
                        {
                            movePoint.position = targetPos;
                        }
                    }
                }

                // moving combo fails if player isn't on beat
                else
                {
                    walkCombo = 0;
                }

                animator.SetTrigger("Move");
            }

            else
            {
                Collider[] enemy = Physics.OverlapSphere(targetPos, .2f, enemies);

                // start npc battle if there is an enemy
                if (enemy.Length > 0)
                {
                    npcObject = enemy[0].transform.gameObject;
                    WorldNpc npc = npcObject.GetComponent<WorldNpc>();
                    npc.hpObject.SetActive(true);

                    battleOn = true;
                    canMove = false;
                    lineBar.color = lineBarBattleColor;

                    RandomBunny();
                }
            }
        }
    }

    public void CheckBattleHit(float moveHori)
    {
        // if songposinbeats is near whole number, player is on rhythm, if near half, player is not on rhythm
        songPosRounded = Mathf.Round(songPosInBeats - beatDiffFix);
        float inaccuracy = Mathf.Abs((songPosInBeats - beatDiffFix) - songPosRounded);

        WorldNpc npc = npcObject.GetComponent<WorldNpc>();

        // left/right button press must match bunny type
        if (moveHori == bunnyValue && inaccuracy < rhythmThreshold)
        {
            npc.TakeHeal();

            for (int i = 0; i < lights.Count; i++)
            {
                StartCoroutine("LightIntensify", lights[i]);
            }
        }

        else
        {
            TakeDamage();
        }

        if (npc.hp < 100)
        {
            RandomBunny();
            canHit = false;
        }
    }

    public void RandomBunny()
    {
        leftBunny.enabled = false;
        rightBunny.enabled = false;

        int randomBunny = Random.Range(0, 2);

        bunnies[randomBunny].enabled = true;
        bunnyValue = bunnyValues[randomBunny];
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

    public void BattleOver()
    {
        leftBunny.enabled = false;
        rightBunny.enabled = false;
        battleOn = false;
        lineBar.color = lineBarOriginalColor;
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
