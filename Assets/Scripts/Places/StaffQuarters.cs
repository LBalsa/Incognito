using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaffQuarters : Place
{
    public List<GameObject> beds;

    private void Reset()
    {
        tag = "Place";
        placeType = PlaceType.staffQuarters;
        GetBeds();
    }

    protected void GetBeds()
    {
        beds.Clear();
        foreach (var child in GetComponentsInChildren<Bed>())
        {
            beds.Add(child.gameObject);
        }
    }
}
