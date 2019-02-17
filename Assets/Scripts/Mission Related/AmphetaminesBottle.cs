using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmphetaminesBottle : MissionObject, IInteractable
{
    // Enable player to pick up the rotten food.
    public void InteractHands()
    {
        if (!triggered)
        {
            triggered = true;
            Map.instance.staffroom.mints.SetActive(true);
            GameManager.instance.Checkpoint(3);
        }
    }

    public void InteractEyes()
    {
    }
}
