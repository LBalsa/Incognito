using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    public bool isLocked = false;

    // Use this for initialization
    //void Start()
    //{
    //    this.name = "Door";
    //}

    public void Open()
    {

    }

    public void InteractHands()
    {
        GetComponent<FPH_DoorObject>().OpenDoor();
        //throw new System.NotImplementedException();
    }

    public void InteractEyes()
    {
        throw new System.NotImplementedException();
    }
}
