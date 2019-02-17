using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionTrigger : MonoBehaviour
{

    public int mission;
    public int checkpointNumber;
    bool triggered = false;

    // Use this for initialization
    //void Start()
    //{
    //    gameObject.SetActive(false);
    //}

    protected void OnTriggerEnter(Collider other)
    {   // Only trigger a checkpoint if it is at the right point in progress.
        if (!triggered && other.CompareTag("Player") && GameManager.instance.missionCheckpoints[mission] == checkpointNumber)
        {
            GameManager.instance.Checkpoint(mission);
            triggered = true;
            //Destroy(this);
        }
    }

    //private void OnTriggerStay(Collider other)
    //{
    //    if (!triggered && other.CompareTag("Player") && GameManager.instance.missionCheckpoints[mission] == checkpointNumber)
    //    {
    //        print("This " + checkpointNumber);
    //        GameManager.instance.Checkpoint(mission);
    //        triggered = true;
    //        Destroy(this);
    //    }
    //}
}
