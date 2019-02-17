using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Office : Place {

    private void Reset()
    {
        tag = "Place";
        placeType = PlaceType.office;
    }
}
