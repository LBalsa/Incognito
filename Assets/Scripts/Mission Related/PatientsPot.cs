using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatientsPot : MissionObject, IInteractable
{
    public void InteractEyes()
    {
        throw new System.NotImplementedException();
    }

    public void InteractHands()
    {
        // Enable player to pick up the rotten food.
        if (!triggered)
        {
            triggered = true;
            Map.instance.kitchen.foulFood.AddComponent<RottenSausages>();
            GameManager.instance.Checkpoint(2);
        }
        Destroy(this);
    }
}
