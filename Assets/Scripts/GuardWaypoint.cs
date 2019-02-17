using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardWaypoint : MonoBehaviour
{
    public int routeNumber;
    public int waypointNumber;
    public float waitTime;
    public enum waypointType
    {
        start, normal, end, wait, shift,
        shiftBack, skipTo, doSomething
    };
    public waypointType type = waypointType.normal;

    [Header("Alt details")]
    public int routeAltNumber;
    public int shiftBackTo;
    public GuardWaypoint skipRef;
}
