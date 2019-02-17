using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RottenSausages : MissionObject, IInteractable
{
    public void InteractEyes()
    {
        // Need a dialogue.
    }

    public void InteractHands()
    {
        if (!triggered)
        {
            // Enable player to drop food in the guard's cooking pot.
            triggered = true;
            Map.instance.kitchen.guardsPot.AddComponent<GuardsPot>();
            GameManager.instance.Checkpoint(2);
            Destroy(this.gameObject);
        }
    }
}
