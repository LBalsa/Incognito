using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clock : MonoBehaviour
{
    public static Clock instance = null;

    bool isFrozen = true;

    public int hour = 8;
    public int minutes = 10;
    int displayHour;
    int day = 1;

    float secondsPerMinute = 2.0f;

    float daySecondsPerMinute = 1.0f;
    float nightSecondsPerMinute = 1.0f;

    float lastChange = 0.0f;

    string ampm = "am";

    void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(this); }

        // Make time move faster at night.
        if (hour >= 0 && hour <= 6) { secondsPerMinute = nightSecondsPerMinute; }
        else { secondsPerMinute = daySecondsPerMinute; }

        DontDestroyOnLoad(this.gameObject);
    }

    void Update()
    {
        instance = this;
        if (Time.time - lastChange > SecondsPerMinute)
        {
            // Make time move faster at night.
            if (hour == 0) { secondsPerMinute = nightSecondsPerMinute; }
            else if (hour == 6) { secondsPerMinute = daySecondsPerMinute; }

            if (!IsFrozen) minutes++;
            if (minutes == 60)
            {
                minutes = 0;
                hour++;
                if (hour == 12 || hour == 24)
                {
                    if (ampm == "am")
                    { ampm = "pm"; }
                    else
                    {
                        ampm = "am";
                        NewDay();
                    }
                }
                if (hour >= 24)
                { NewDay(); }
            }
            lastChange = Time.time;
        }
    }

    void OnGUI()
    {
        displayHour = hour;
        if (displayHour > 12)
        {
            displayHour -= 12;
        }
        if (displayHour < 10) GUILayout.Label("0" + displayHour.ToString() + ":" + minutes.ToString() + " " + ampm);
        else if (minutes < 10) GUILayout.Label(displayHour.ToString() + ":" + minutes.ToString() + "0 " + ampm);
        else GUILayout.Label(displayHour.ToString() + ":" + minutes.ToString() + " " + ampm);
    }

    void NewDay()
    {
        hour = 0;
        day++;
    }

    public bool IsFrozen
    {
        get
        { return isFrozen; }
        set
        { isFrozen = value; }
    }

    public float SecondsPerMinute
    {
        get
        { return secondsPerMinute; }
        set
        { secondsPerMinute = value; }
    }

    public void SetClock(int h, int m)
    { hour = h; minutes = m; }

    public Vector2Int GetClock()
    { return new Vector2Int(hour, minutes); }
    public int GetClockHour()
    { return hour; }
    public int GetClockMinute()
    { return minutes; }
}