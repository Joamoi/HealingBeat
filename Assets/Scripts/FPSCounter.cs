using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    public TextMeshProUGUI fpsDisplay;
    private float fps;
    private int fpsInt;
    private float timer;

    void Start()
    {
        timer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        // we use unscaled because sometimes normal deltatime is modified for slow-motion etc
        fps = 1 / Time.unscaledDeltaTime;

        // calculates new value for fps every given time period
        if (timer > 0.5f)
        {
            fpsInt = (int)fps;
            fpsDisplay.text = "" + fpsInt;
            timer = 0;
        }
    }
}
