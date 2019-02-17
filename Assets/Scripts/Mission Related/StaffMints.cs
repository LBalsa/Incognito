using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaffMints : MissionObject, IInteractable
{
    public void InteractHands()
    {
        if (!triggered)
        {
            triggered = true;
            GameManager.instance.Checkpoint(3);
        }
    }

    public void InteractEyes()
    {
    }
}
