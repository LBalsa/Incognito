using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kitchen : Place
{
    public GameObject patientsPot;
    public GameObject guardsPot;
    public GameObject foulFood;
    public GameObject goodFood;

    private void Reset()
    {
        tag = "Place";
        placeType = PlaceType.kitchen;
    }
}
