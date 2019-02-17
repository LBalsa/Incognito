using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardsPot : MissionObject, IInteractable
{
    bool done = false;
    public void InteractEyes()
    {
        if (!triggered)
        {
            // "This must be the guard's own food."
        }
        else
        {
            // "That'll show them..."
        }
        throw new System.NotImplementedException();
    }

    public void InteractHands()
    {
        // Mission complete.
        if (!triggered)
        {
            triggered = true;
            Map.instance.kitchen.guardsPot.AddComponent<GuardsPot>();
            GameManager.instance.Checkpoint(2);
            Destroy(this);
        }
    }
}
