using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour
{
    public GameObject spawnPos;
    public GameObject destroyPos;
    public GameObject parentObject;
    public GameObject[] objects;

    public float minInterval;
    public float maxInterval;
    public static float backgroundMoveSpeed = 2f;
    public float startMultiplier;
    private int previous = 99;
    private int randomBuilding;

    private float timer;
    private float nextSpawn;

    // Start is called before the first frame update
    void Start()
    {
        float pathLength = Mathf.Abs(destroyPos.transform.position.x - spawnPos.transform.position.x);

        for (float i = 0; i < (pathLength - maxInterval);)
        {
            float interval = Random.Range(minInterval, maxInterval) * startMultiplier;

            do
            {
                randomBuilding = Random.Range(0, 5);
            }
            while (previous == randomBuilding);

            GameObject newObject = Instantiate(objects[randomBuilding], parentObject.transform);
            newObject.transform.position = spawnPos.transform.position - new Vector3(i + interval, 0f, 0f);

            i = i + interval;
            previous = randomBuilding;
        }

        timer = 0f;
        nextSpawn = Random.Range(minInterval, maxInterval);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        
        if(timer >= nextSpawn)
        {
            do
            {
                randomBuilding = Random.Range(0, 5);
            }
            while (previous == randomBuilding);

            GameObject newObject = Instantiate(objects[randomBuilding], parentObject.transform);
            newObject.transform.position = spawnPos.transform.position;

            nextSpawn = timer + Random.Range(minInterval, maxInterval);
            previous = randomBuilding;
        }
    }
}
