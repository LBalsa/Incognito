using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayRoom : Place
{
    public List<Transform> shelves;

    void Start()
    {
        if (tag != "Place" || placeType != PlaceType.dayRoom)
        {
            Debug.LogError("Place marked incorrectly: " + name);
            Reset();
            Map.instance.dayRoom = this;
        }

    }

    protected void GetShelves()
    {
        shelves.Clear();
        foreach (var child in GetComponentsInChildren<Shelf>())
        {
            shelves.Add(child.transform);
        }
    }
    private void Reset()
    {
        tag = "Place";
        placeType = PlaceType.dayRoom;

        GetSeats();
        GetShelves();
    }
}
