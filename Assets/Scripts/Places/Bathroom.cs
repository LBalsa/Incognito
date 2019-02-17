using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bathroom : Place
{
    // Use this for initialization
    void Start()
    {
        /*
        if (this.tag != "Place" || placeType != PlaceType.bathroom)
        {
            tag = "Place";
            placeType = PlaceType.bathroom;
            Map.instance.bathRooms.Add(this);
        }
        */
        // Ensure place has trigger

    }

    private void Reset()
    {
        tag = "Place";
        //placeType = PlaceType.bathroomCentre;
        GetComponent<BoxCollider>().isTrigger = true;


        GetSeats();
        GetWaitpoints();
    }
}
