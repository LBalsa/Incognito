using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : Place
{
    public int cellNumber;
    public Door door;
    public GameObject bed;
    public GameObject bedsideTable;

    void Start()
    {
        if (!added)
        {
            placeType = PlaceType.cell;
            Map.instance.cells.Add(this);
        }

        /*
        cellNumber = cellCount;
        cellCount++;
        */

        GetSeats();
        bed = GetComponentInChildren<Bed>().gameObject;
        bedsideTable = GetComponentInChildren<BedsideTable>().gameObject;
        door = GetComponentInChildren<Door>();
    }


    private void Reset()
    {
        tag = "Place";
        placeType = PlaceType.cell;
        GetSeats();
        bed = GetComponentInChildren<Bed>().gameObject;
        bedsideTable = GetComponentInChildren<BedsideTable>().gameObject;
        door = GetComponentInChildren<Door>();
    }
}
