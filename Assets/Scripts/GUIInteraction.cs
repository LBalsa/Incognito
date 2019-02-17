using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIInteraction : MonoBehaviour
{
    public static GUIInteraction inst;

    void Awake()
    {
        if (inst == null) inst = this;
        else Destroy(this);
        this.gameObject.SetActive(false);
    }

}
