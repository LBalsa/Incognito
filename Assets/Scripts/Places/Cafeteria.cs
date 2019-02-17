using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cafeteria : Place
{
    [Header("Cafeteria")]
    //public Transform queue;
    public Waitpoint trayPickup;
    public Waitpoint trayDropoff;
    public Waitpoint foodPoint;

    // Use this for initialization
    void Start()
    {
        if (this.tag != "Place" || placeType != PlaceType.cafeteria)
        {
            tag = "Place";
            placeType = PlaceType.cafeteria;
            Map.instance.cafeteria = this;
        }
    }

    private void Reset()
    {
        tag = "Place";
        placeType = PlaceType.cafeteria;
        GetSeats();
        CheckTrayPoints();
        // Ensure place is a trigger.
        GetComponent<BoxCollider>().isTrigger = true;
    }

    void CheckTrayPoints()
    {
        // Ensure waitpoints are assigned.
        if (!trayPickup || !trayDropoff || !foodPoint)
        {
            foreach (Waitpoint wp in GetComponentsInChildren<Waitpoint>())
            {
                if (wp.name == "TrayPickup")
                {
                    trayPickup = wp;
                }
                else if (wp.name == "TrayDropoff")
                {
                    trayDropoff = wp;
                }
                else if (wp.name == "FoodPickup")
                {
                    foodPoint = wp;
                }

            }
        }

    }

}
