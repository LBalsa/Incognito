using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaffRoom : Place
{
    public GameObject mints;
    private void Reset()
    {
        tag = "Place";
        placeType = PlaceType.staffRoom;
        GetSeats();
    }
}
