using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Therapy : Place
{

    // Use this for initialization
    void Start()
    {
        GetSeats();
    }
    private void Reset()
    {
        tag = "Place";
        //placeType = PlaceType.therapy1;
        GetSeats();
    }

}
