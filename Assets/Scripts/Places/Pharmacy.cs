using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pharmacy : Place
{
    public GameObject medicineCupboard;

    private void Reset()
    {
        tag = "Place";
        placeType = PlaceType.pharmacy;
    }
}
