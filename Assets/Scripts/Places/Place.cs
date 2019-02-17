using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Place : MonoBehaviour
{
    public enum PlaceType
    {
        home, reception, hall, cafeteria, cell,
        bathroomCentre, bathroomWest, bathroomEast,
        shower, dayRoom, bed, kitchen,
        therapy1, therapy2, therapy3, therapy4, therapy5, therapy6,
        staffRoom, staffQuarters, staffToilet, office, pharmacy, records
    }

    public PlaceType placeType;

    public List<Seat> seats;
    public List<Waitpoint> sinks;
    public List<Waitpoint> toilets;
    public List<Waitpoint> showers;


    [HideInInspector]
    public bool added = false;

    private void Awake()
    {
        this.tag = "Place";
        // Ensure place has trigger
        GetComponent<BoxCollider>().isTrigger = true;
        GetSeats();
        //GetBeds();
    }


    protected void GetSeats()
    {
        seats.Clear();
        foreach (var child in GetComponentsInChildren<Seat>())
        {
            seats.Add(child);
        }
    }
    //protected void GetBeds()
    //{
    //    beds.Clear();
    //    foreach (var child in GetComponentsInChildren<Bed>())
    //    {
    //        beds.Add(child);
    //    }
    //}

    protected void GetWaitpoints()
    {
        sinks.Clear();
        toilets.Clear();
        showers.Clear();

        foreach (var wp in GetComponentsInChildren<Waitpoint>())
        {
            switch (wp.type)
            {
                case Waitpoint.Type.Caf:
                    break;
                case Waitpoint.Type.sink:
                    sinks.Add(wp);
                    break;
                case Waitpoint.Type.toilet:
                    toilets.Add(wp);
                    break;
                case Waitpoint.Type.shower:
                    showers.Add(wp);
                    break;
                default:
                    break;
            }
        }
    }

}




