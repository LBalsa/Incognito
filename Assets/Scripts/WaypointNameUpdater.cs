using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class WaypointNameUpdater : MonoBehaviour
{

    string newName;

#if UNITY_EDITOR
    void Update()
    {

        GuardWaypoint[] wps = gameObject.GetComponents<GuardWaypoint>();
        newName = "";
        for (int i = 0; i < wps.Length; i++)
        {
            if (i == wps.Length - 1)
            {
                newName = newName + "G" + wps[i].routeNumber.ToString() + "-" + wps[i].waypointNumber.ToString() + "-" + wps[i].type;
                break;
            }
            newName = newName + "G" + wps[i].routeNumber.ToString() + "-" + wps[i].waypointNumber.ToString() + "-" + wps[i].type + "\n";
        }

        name = newName;
    }

#endif


}
