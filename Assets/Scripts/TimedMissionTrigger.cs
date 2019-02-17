using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedMissionTrigger : MissionTrigger
{
    public Vector2Int timeOn;
    public Vector2Int timeOff;
    public bool on;
    // Use this for initialization
    void Start()
    {
        if (Clock.instance.hour >= timeOn.x && Clock.instance.hour <= timeOff.x)
        {
            GetComponent<BoxCollider>().enabled = true;
        }
        InvokeRepeating("CheckTime", 1, 1);
    }

    void CheckTime()
    {
        if (Clock.instance.hour == timeOn.x && Clock.instance.minutes == timeOn.y)
        {
            GetComponent<BoxCollider>().enabled = true;
        }
        else if (Clock.instance.hour == timeOff.x && Clock.instance.minutes == timeOff.y)
        {
            GetComponent<BoxCollider>().enabled = false;
        }

    }
}
