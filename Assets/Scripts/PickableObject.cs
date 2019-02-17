using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickableObject : Obj, IInteractable
{

    public Vector3 HandPos;
    public Vector3 HandRot;

    void IInteractable.InteractEyes()
    {
        Player.instance.Hmm();
    }

    void IInteractable.InteractHands()
    {
        //gameObject.SetActive(false);
        Player.instance.PickUp(gameObject);
    }

}
