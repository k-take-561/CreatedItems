using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    int minute;
    float seconds;
    float oldSeconds;
    Text time;
    GameObject goal;
    Goal script;
    // Start is called before the first frame update
    void Start()
    {
        minute = 0;
        seconds = 0f;
        oldSeconds = 0f;
        time = GetComponent<Text>();
        goal = GameObject.Find("Goal");
        script = goal.GetComponent<Goal>();
    }

    // Update is called once per frame
    void Update()
    {
        seconds += Time.deltaTime;
        if(seconds >= 60f)
        {
            minute++;
            seconds -= 60f;
        }
        if(script.flg == false)time.text = minute.ToString("00") + ":" + seconds.ToString();
        oldSeconds = seconds;

    }
}
