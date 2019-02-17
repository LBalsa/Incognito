using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class stores all the places in the game in order to provide NPCs with their location 
// without them having to store it individually and wasting memory.

public class Map : MonoBehaviour
{
    public static Map instance = null;

    // Lists of places;
    public Cafeteria cafeteria;
    public DayRoom dayRoom;
    public Kitchen kitchen;

    public Place hall;
    public List<Cell> cells = new List<Cell>();
    public Bathroom bathroomCentre, bathroomWest, bathroomEast;
    public List<Bathroom> bathRooms = new List<Bathroom>();
    public List<Place> showerRooms = new List<Place>();

    public Therapy therapy1, therapy2, therapy3, therapy4, therapy5, therapy6;

    public Office office;
    public Pharmacy pharmacy;
    public Records records;
    public StaffRoom staffroom;
    public StaffQuarters staffquarters;

    // Use this for initialization
    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);

        // Compile lists of places;
        CompileLists();
    }

    void CompileLists()
    {
        // Find all places and add them to correct list.
        foreach (var p in GameObject.FindGameObjectsWithTag("Place"))
        {
            switch (p.GetComponent<Place>().placeType)
            {
                case Place.PlaceType.hall:
                    hall = p.GetComponent<Place>();
                    break;
                case Place.PlaceType.cafeteria:
                    cafeteria = p.GetComponent<Cafeteria>();
                    break;
                case Place.PlaceType.kitchen:
                    kitchen = p.GetComponent<Kitchen>();
                    break;
                case Place.PlaceType.cell:
                    cells.Add(p.GetComponent<Cell>());
                    break;

                // Bathrooms.
                case Place.PlaceType.bathroomCentre:
                    bathroomCentre = p.GetComponent<Bathroom>();
                    bathRooms.Add(p.GetComponent<Bathroom>());
                    p.GetComponent<Place>().added = true;
                    break;
                case Place.PlaceType.bathroomEast:
                    bathroomEast = p.GetComponent<Bathroom>();
                    bathRooms.Add(p.GetComponent<Bathroom>());
                    break;
                case Place.PlaceType.bathroomWest:
                    bathroomWest = p.GetComponent<Bathroom>();
                    bathRooms.Add(p.GetComponent<Bathroom>());
                    break;
                case Place.PlaceType.shower:
                    showerRooms.Add(p.GetComponent<Place>());
                    break;

                // Therapy Rooms
                case Place.PlaceType.therapy1:
                    therapy1 = p.GetComponent<Therapy>();
                    break;
                case Place.PlaceType.therapy2:
                    therapy2 = p.GetComponent<Therapy>();
                    break;
                case Place.PlaceType.therapy3:
                    therapy3 = p.GetComponent<Therapy>();
                    break;
                case Place.PlaceType.therapy4:
                    therapy4 = p.GetComponent<Therapy>();
                    break;
                case Place.PlaceType.therapy5:
                    therapy5 = p.GetComponent<Therapy>();
                    break;
                case Place.PlaceType.therapy6:
                    therapy6 = p.GetComponent<Therapy>();
                    break;
                // Other rooms.
                case Place.PlaceType.dayRoom:
                    dayRoom = p.GetComponent<DayRoom>();
                    break;

                // Staff
                case Place.PlaceType.pharmacy:
                    pharmacy = p.GetComponent<Pharmacy>();
                    break;
                case Place.PlaceType.records:
                    records = p.GetComponent<Records>();
                    break;
                case Place.PlaceType.office:
                    office = p.GetComponent<Office>();
                    break;
                case Place.PlaceType.staffRoom:
                    staffroom = p.GetComponent<StaffRoom>();
                    break;
                case Place.PlaceType.staffQuarters:
                    staffquarters = p.GetComponent<StaffQuarters>();
                    break;
                default:
                    break;
            }
            p.GetComponent<Place>().added = true;
        }
    }
}
