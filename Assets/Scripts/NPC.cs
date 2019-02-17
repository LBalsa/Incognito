using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using AdvancedCoroutines;

public class NPC : MonoBehaviour
{
    #region Bools
    protected bool nPCDebug = false;
    protected bool isAwake;
    protected bool isAsleep = false;
    protected bool isGoingTo = false;
    [SerializeField]
    protected bool isBusy = false;
    protected bool hasTray = false;
    protected bool read = false;
    #endregion

    protected int hour, minute;

    protected float health;
    protected float stamina;
    protected float hunger;
    protected float speedWalk = 2.5f;
    protected float speedRun = 3.5f;
    protected float waitTime = 0;

    public enum Wings { Central, East, West }
    public Wings wing;

    #region Components
    public Place home = null;
    protected Place place = new Place();
    protected Place currentPlace = null;
    protected Seat currentSeat = null;

    protected Animator animator;
    protected AudioSource audioS;
    protected GameObject hand;
    protected GameObject currentQueue;
    protected GameObject nPCHeader;
    protected TextMesh t;
    protected NavMeshAgent navMeshAgent;
    #endregion

    #region WaitForSeconds
    protected WaitForSeconds sec025 = new WaitForSeconds(.25f);
    protected WaitForSeconds sec05 = new WaitForSeconds(.5f);
    protected WaitForSeconds sec1 = new WaitForSeconds(1);
    protected WaitForSeconds sec2 = new WaitForSeconds(2);
    protected WaitForSeconds sec3 = new WaitForSeconds(3);
    protected WaitForSeconds sec5 = new WaitForSeconds(5);
    protected WaitForSeconds sec10 = new WaitForSeconds(10);
    #endregion

    public FXStructure fX;

    private void Awake()
    {
        try
        {
            //nPCDebug = GameManager.instance.nPCDebug;
            animator = GetComponent<Animator>();
        }
        catch (System.NullReferenceException)
        {
            throw;
        }
    }

    virtual protected void DrawPath()
    {
        for (int i = 0; i < navMeshAgent.path.corners.Length - 1; i++)
        {
            Debug.DrawLine(navMeshAgent.path.corners[i], navMeshAgent.path.corners[i + 1], Color.gray);
        }
    }

    protected IEnumerator WakeUp()
    {
        if (this.tag == "Patient")
        {
            // Wake up animation and delay.
            animator.SetTrigger("GetUp");
            yield return sec10;

            // Enable navmesh agent.
            navMeshAgent.enabled = true;
            navMeshAgent.isStopped = false;
            navMeshAgent.updateRotation = true;

            // Go to bedside table.
            GameObject bedside = home.GetComponent<Cell>().bedsideTable;
            navMeshAgent.destination = bedside.transform.position;
            yield return new WaitUntil(() => Vector3.Distance(transform.position, bedside.transform.position) <= 1);
            animator.SetLayerWeight(1, 1);
            animator.SetTrigger("UseHand");
            yield return sec3;
            animator.SetLayerWeight(1, 0);

            // Go to toilet.
            Waitpoint sink;
            switch (wing)
            {
                case Wings.Central:
                    sink = FindRandomWaitpoint(Map.instance.bathroomCentre.sinks, true);
                    break;
                case Wings.East:
                    sink = FindRandomWaitpoint(Map.instance.bathroomEast.sinks, true);
                    break;
                case Wings.West:
                    sink = FindRandomWaitpoint(Map.instance.bathroomWest.sinks, true);
                    break;
                default:
                    sink = FindRandomWaitpoint(Map.instance.bathroomCentre.sinks, true);
                    break;
            }
            yield return StartCoroutine(WaitForWaitpoint(sink));
            PlaySFX(fX.tap);
            animator.SetLayerWeight(1, 1);
            animator.SetTrigger("UseHand");
            yield return new WaitForSeconds(animator.GetCurrentAnimatorClipInfo(0).Length);
            animator.SetLayerWeight(1, 0);
            sink.IsBusy = false;

            // Go to bedside table.
            UpdateHeader(" going to room");
            navMeshAgent.destination = bedside.transform.position;
            yield return new WaitUntil(() => Vector3.Distance(transform.position, bedside.transform.position) <= 1);
            animator.SetLayerWeight(1, 1);
            animator.SetTrigger("UseHand");
            yield return sec3;
            animator.SetLayerWeight(1, 0);

            isBusy = false;
            UpdateState(Patient.PatientState.idle);
        }
        yield return null;
    }

    protected IEnumerator GoToSleep()
    {
        UpdateHeader(" going to sleep");
        GameObject bed = null;
        if (this.tag == "Patient")
        {
            bed = home.GetComponent<Cell>().bed;
            UpdateState(Patient.PatientState.sleep);
        }
        else if (this.tag == "Guard")
        {
            bed = Map.instance.staffquarters.beds[GetComponent<Guard>().guardNumber];
            UpdateState(Guard.GuardState.passive);
        }
        navMeshAgent.destination = bed.transform.position;
        yield return new WaitUntil(() => Vector3.Distance(transform.position, bed.transform.position) <= 1);
        navMeshAgent.isStopped = true;
        animator.SetTrigger("LayDown");
    }

    protected void Eat()
    {

    }

    protected IEnumerator UseHands()
    {
        animator.SetLayerWeight(1, 1);
        animator.SetTrigger("UseHand");
        yield return sec3;
        animator.SetLayerWeight(1, 0);
    }

    void GoToPlace(Place.PlaceType place)
    {
        switch (place)
        {
            case Place.PlaceType.home:
                navMeshAgent.destination = home.transform.position;
                break;
            case Place.PlaceType.hall:
                navMeshAgent.destination = Map.instance.hall.transform.position;
                break;
            case Place.PlaceType.cafeteria:
                navMeshAgent.destination = Map.instance.cafeteria.transform.position;
                break;
            case Place.PlaceType.shower:
                break;
            case Place.PlaceType.dayRoom:
                break;
            default:
                break;
        }
    }

    virtual protected void UpdateHeader(string debug)
    {
        if (!nPCHeader) nPCHeader = transform.GetChild(0).gameObject;

        if (nPCDebug) Debug.Log(name + debug);

        nPCHeader.name = name + debug;
    }


    protected void GoToPlaceSwitch(Place place)
    {
        switch (place.placeType)
        {
            case Place.PlaceType.home:
                navMeshAgent.destination = home.transform.position;
                break;
            case Place.PlaceType.hall:
                navMeshAgent.destination = Map.instance.hall.transform.position;
                break;
            case Place.PlaceType.cafeteria:
                navMeshAgent.destination = Map.instance.cafeteria.transform.position;
                break;
            case Place.PlaceType.bathroomCentre:
                switch (wing)
                {
                    case Wings.Central:
                        navMeshAgent.destination = Map.instance.bathroomCentre.transform.position;
                        break;
                    case Wings.East:
                        navMeshAgent.destination = Map.instance.bathroomEast.transform.position;
                        break;
                    case Wings.West:
                        navMeshAgent.destination = Map.instance.bathroomWest.transform.position;
                        break;
                    default:
                        break;
                }
                break;
            case Place.PlaceType.bathroomEast:
                switch (wing)
                {
                    case Wings.Central:
                        navMeshAgent.destination = Map.instance.bathroomCentre.transform.position;
                        break;
                    case Wings.East:
                        navMeshAgent.destination = Map.instance.bathroomEast.transform.position;
                        break;
                    case Wings.West:
                        navMeshAgent.destination = Map.instance.bathroomWest.transform.position;
                        break;
                    default:
                        break;
                }
                break;
            case Place.PlaceType.bathroomWest:
                switch (wing)
                {
                    case Wings.Central:
                        navMeshAgent.destination = Map.instance.bathroomCentre.transform.position;
                        break;
                    case Wings.East:
                        navMeshAgent.destination = Map.instance.bathroomEast.transform.position;
                        break;
                    case Wings.West:
                        navMeshAgent.destination = Map.instance.bathroomWest.transform.position;
                        break;
                    default:
                        break;
                }
                break;
            case Place.PlaceType.shower:
                navMeshAgent.destination = Map.instance.showerRooms[0].transform.position;
                break;
            case Place.PlaceType.dayRoom:
                navMeshAgent.destination = Map.instance.dayRoom.transform.position;
                break;
            case Place.PlaceType.therapy1:
                navMeshAgent.destination = Map.instance.therapy1.transform.position;
                break;
            case Place.PlaceType.therapy2:
                navMeshAgent.destination = Map.instance.therapy2.transform.position;
                break;
            case Place.PlaceType.therapy3:
                navMeshAgent.destination = Map.instance.therapy3.transform.position;
                break;
            case Place.PlaceType.therapy4:
                navMeshAgent.destination = Map.instance.therapy4.transform.position;
                break;
            case Place.PlaceType.therapy5:
                navMeshAgent.destination = Map.instance.therapy5.transform.position;
                break;
            case Place.PlaceType.therapy6:
                navMeshAgent.destination = Map.instance.therapy6.transform.position;
                break;
            case Place.PlaceType.bed:
                StartCoroutine("GoToSleep");
                break;
            default:
                break;
        }
    }


    #region Arriving functions

    protected IEnumerator ArriveCafeteria(Cafeteria place)
    {
        UpdateHeader(" entering cafeteria");

        // Get food tray.
        yield return StartCoroutine(GetTray(place));

        yield return StartCoroutine(WaitpointQueue(place.foodPoint));
        place.foodPoint.IsBusy = false;

        // Find a vacant seat.
        Seat seat = FindRandomSeat(place);

        // Wander and wait if no seats are available.
        while (!seat)
        {
            UpdateHeader(" looking for seat");

            Vector2 circle = Random.insideUnitCircle;
            navMeshAgent.SetDestination(new Vector3(circle.x * 2 + transform.position.x, 0f, circle.y * 2 + transform.position.z));
            yield return sec3;

            seat = FindSeat(place);
        }

        yield return StartCoroutine(Sit(seat));
        UpdateState(Patient.PatientState.sittingEating);
        UpdateHeader(" eating");

        isBusy = false;
    }

    protected IEnumerator ExitCafeteria(Cafeteria place)
    {
        isBusy = true;
        UpdateHeader(" exiting cafeteria");
        yield return StartCoroutine(Stand(false));

        // Dropoff food tray.
        yield return StartCoroutine(DropTray(place));

        isBusy = false;
    }

    protected IEnumerator ArriveBathroom(Bathroom place)
    {
        UpdateHeader(" entering bathroom");

        // Get toilet.
        Waitpoint toilet = FindRandomWaitpoint(place.toilets, true);

        yield return StartCoroutine(WaitpointQueue(toilet));
        PlaySFX(fX.pee);
        yield return StartCoroutine("UseHands");
        toilet.IsBusy = false;

        // Get sink.
        Waitpoint sink = FindRandomWaitpoint(place.sinks, true);

        yield return StartCoroutine(WaitpointQueue(sink));
        PlaySFX(fX.tap);
        yield return StartCoroutine("UseHands");

        sink.IsBusy = false;

        isBusy = false;

        SetIdle();
    }

    protected IEnumerator ArriveDayroom(DayRoom place)
    {
        yield return sec2;

        // Find a vacant seat.
        Seat seat = FindRandomSeat(place);

        // Wander and wait if no seats are available.
        while (!seat)
        {
            UpdateHeader(" looking for seat");

            Vector2 circle = Random.insideUnitCircle;
            navMeshAgent.SetDestination(new Vector3(circle.x * 2 + transform.position.x, 0f, circle.y * 2 + transform.position.z));
            yield return sec3;
            seat = FindSeat(place);
        }
        yield return StartCoroutine(Sit(seat));
        UpdateState(Patient.PatientState.sittingIdle);
        isBusy = false;
    }

    #endregion



    #region Action functions

    protected Waitpoint FindRandomWaitpoint(List<Waitpoint> waitpoints, bool returnEvenIfBusy)
    {
        // Compile list of free waipoints in area.
        List<Waitpoint> freeWaitpoints = new List<Waitpoint>();
        foreach (var wp in waitpoints)
        {
            if (!wp.IsBusy)
            {
                freeWaitpoints.Add(wp);
            }
        }
        // If there are any free ones, return a random one.
        if (freeWaitpoints.Count != 0)
        {
            return freeWaitpoints[Random.Range(0, freeWaitpoints.Count - 1)];
        }
        // Override for when there are no free ones.
        else if (returnEvenIfBusy) return waitpoints[Random.Range(0, waitpoints.Count - 1)];
        else return null;
    }


    protected Seat FindSeat(Place place)
    {
        //Check seats in area.        
        foreach (var seat in place.seats)
        {
            // If seat is free.
            if (!seat.GetComponent<Seat>().taken)
            {
                // Claim seat.
                seat.GetComponent<Seat>().taken = true;
                currentSeat = seat.GetComponent<Seat>();
                return seat;
            }
        }
        Debug.LogWarning("No seats available");

        return null;
    }

    protected Seat FindRandomSeat(Place place)
    {
        // Compile list of free seats in area.
        List<Seat> freeSeats = new List<Seat>();
        foreach (var seat in place.seats)
        {
            if (!seat.taken) { freeSeats.Add(seat); }
        }
        if (freeSeats.Count != 0)
        {
            // Claim random seat.
            Seat seat = freeSeats[Random.Range(0, freeSeats.Count)];
            seat.taken = true;
            currentSeat = seat;
            return seat;
        }
        else return null;
    }

    protected virtual IEnumerator Sit(Seat seat)
    {
        if (!seat)
        {
            Debug.LogError("No seat provided");
            yield return null;
        }
        // Move to seat.
        Vector3 targetPos = new Vector3(seat.transform.position.x, transform.position.y, seat.transform.position.z);
        navMeshAgent.destination = targetPos;
        yield return new WaitUntil(() => Vector3.Distance(transform.position, targetPos) < 0.5f);

        // Stop navmesh from interfering.
        navMeshAgent.enabled = false;
        GetComponent<NavMeshObstacle>().enabled = true;

        // Match own y axis and seat's to sit properly.
        transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.x, seat.transform.eulerAngles.y, transform.rotation.z));
        transform.position = new Vector3(seat.transform.position.x, transform.position.y, seat.transform.position.z);
        animator.SetTrigger("Sit");

        UpdateState(Patient.PatientState.sittingIdle);
        isBusy = false;
    }

    protected virtual IEnumerator Stand(bool resetBusy)
    {
        animator.SetTrigger("Stand");
        yield return sec3;
        // Mark seat as free.
        currentSeat.taken = false;
        currentSeat = null;
        // Turn on navmesh agent.
        GetComponent<NavMeshObstacle>().enabled = false;
        navMeshAgent.enabled = true;
        if (resetBusy)
        {
            isBusy = false;
        }
    }


    protected virtual IEnumerator Talk()
    {
        nPCHeader.name = name + " talking";

        animator.SetLayerWeight(1, 1);
        animator.SetTrigger("Talk");
        animator.SetLayerWeight(1, 0);
        yield return sec5;

    }


    protected IEnumerator WaitForWaitpoint(Waitpoint waitpoint)
    {
        yield return waitpoint.IsBusy = false;
        yield return StartCoroutine(WaitpointQueue(waitpoint));
    }

    // NPCs queue when they notice someone has arrived before them.
    protected IEnumerator Waitpoint(Waitpoint waitpoint)
    {
        bool wasBusy = false; // Loop control variable.

        navMeshAgent.stoppingDistance = 1;
        navMeshAgent.autoBraking = true;
        Vector3 destination = new Vector3(waitpoint.transform.position.x, transform.position.y, waitpoint.transform.position.z); ;

        // Check if waitpoint is free and move to them.
        if (!waitpoint.IsBusy)
        {
            UpdateHeader(" going to " + waitpoint.name);
            navMeshAgent.SetDestination(destination);
        }

        // Continue checking if someone else is picking up a tray until close.
        do
        {
            // If someone reaches point before self:
            if (waitpoint.IsBusy)
            {
                wasBusy = true;

                // Queue behind them.
                currentQueue = waitpoint.current;

                // Set queue point to self.
                waitpoint.current = this.gameObject;
                // Add self to queue.
                waitpoint.queue.Enqueue(this.gameObject);
                waitpoint.queueLenght = waitpoint.queue.Count;

                UpdateHeader(" waiting " + waitpoint.name);

                while (true)
                {
                    // Wait until waitpoint is free, keep in queue.
                    if (waitpoint.IsBusy)
                    {
                        if (waitpoint.queue.Peek() == this.gameObject && waitpoint.queuePoint != null)
                        {
                            navMeshAgent.SetDestination(waitpoint.queuePoint.position);

                        }
                        else
                        {
                            navMeshAgent.SetDestination(currentQueue.transform.position);
                        }
                    }
                    // If free and self is next in queue, dequeue self, set trays as busy and break.
                    else if (!waitpoint.IsBusy && waitpoint.queue.Peek() == this.gameObject)
                    {
                        UpdateHeader(" going to " + waitpoint.name);

                        waitpoint.queue.Dequeue();
                        waitpoint.queueLenght = waitpoint.queue.Count;
                        waitpoint.IsBusy = true;
                        navMeshAgent.SetDestination(destination);

                        break;
                    }
                    yield return sec025;
                }
                break;
            }
            if (wasBusy) break;

            yield return 1;
        } while (Vector3.Distance(transform.position, destination) > 0.5f);

        // If there is no queue, create one;
        if (!wasBusy) waitpoint.current = this.gameObject;
        waitpoint.IsBusy = true;

        navMeshAgent.stoppingDistance = 0.5f;
        navMeshAgent.autoBraking = false;

        if (waitpoint.waitTime > 0)
        {
            yield return new WaitForSeconds(waitpoint.waitTime);
        }
    }


    // NPCs start queuing as soon as they enter.
    protected IEnumerator WaitpointQueue(Waitpoint waitpoint)
    {
        bool wasBusy = false; // Loop control variable.
        bool imFirst = false;

        navMeshAgent.stoppingDistance = 0.5f;
        navMeshAgent.autoBraking = true;
        Vector3 destination = new Vector3(waitpoint.transform.position.x, transform.position.y, waitpoint.transform.position.z);

        // Check if waitpoint is free and move to them.
        if (!waitpoint.IsBusy && waitpoint.queue.Count == 0)
        {
            imFirst = true;
            waitpoint.IsBusy = true;
            navMeshAgent.SetDestination(destination);

            // If there is no queue, create one;
            if (!wasBusy) waitpoint.current = this.gameObject;
        }

        // Continue checking if someone else is picking up a tray until close.
        do
        {
            // If someone reaches point before self:
            if (waitpoint.IsBusy && !imFirst || !waitpoint.IsBusy && waitpoint.queue.Count != 0 && !imFirst)
            {
                wasBusy = true;
                // Prevent AI pushing.
                navMeshAgent.stoppingDistance = 0.8f;
                navMeshAgent.radius = 0.3f;
                // Queue behind them.
                currentQueue = waitpoint.current;
                // Set queue point to self.
                waitpoint.current = this.gameObject;
                // Add self to queue.
                waitpoint.queue.Enqueue(this.gameObject);
                waitpoint.queueLenght = waitpoint.queue.Count;

                while (true)
                {
                    // Wait until waitpoint is free, keep in queue.
                    if (waitpoint.IsBusy)
                    {
                        // If self is first in queue and there is a queuepoint, move to queuepoint.
                        if (waitpoint.queue.Peek() == this.gameObject && waitpoint.queuePoint != null)
                        {
                            navMeshAgent.SetDestination(waitpoint.queuePoint.transform.position);
                        }
                        // Else move behind queue
                        else
                        {
                            navMeshAgent.SetDestination(currentQueue.transform.position);
                        }
                    }
                    // If free and self is next in queue, dequeue self, set waitpoint as busy and break.
                    else if (!waitpoint.IsBusy && waitpoint.queue.Peek() == this.gameObject)
                    {
                        UpdateHeader(" going to " + waitpoint.name);

                        waitpoint.queue.Dequeue();
                        waitpoint.queueLenght = waitpoint.queue.Count;
                        waitpoint.IsBusy = true;
                        navMeshAgent.SetDestination(destination);
                        navMeshAgent.stoppingDistance = 0.5f;
                        navMeshAgent.radius = 0.4f;
                        break;
                    }
                    yield return sec025;
                }
                break;
            }
            navMeshAgent.autoBraking = false;
            yield return 1;
        } while (Vector3.Distance(transform.position, destination) > 0.55f);

        Vector3 target = new Vector3(waitpoint.transform.parent.position.x, transform.position.y, waitpoint.transform.parent.position.z);

        if (waitpoint.waitTime > 0)
        {
            yield return new WaitForSeconds(waitpoint.waitTime);
        }
    }

    private void FaceTarget(Vector3 destination)
    {
        Vector3 lookPos = destination - transform.position;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 1);
    }

    protected IEnumerator GetTray(Cafeteria cafeteria)
    {
        yield return StartCoroutine(WaitpointQueue(cafeteria.trayPickup));

        animator.SetLayerWeight(1, 1);
        animator.SetTrigger("UseHand");
        yield return sec3;
        animator.SetLayerWeight(1, 0);

        cafeteria.trayPickup.IsBusy = false;
        hasTray = true;
    }

    protected IEnumerator DropTray(Cafeteria cafeteria)
    {
        yield return StartCoroutine(Waitpoint(cafeteria.trayDropoff));

        animator.SetLayerWeight(1, 1);
        animator.SetTrigger("DropoffTray");
        yield return sec3;
        animator.SetLayerWeight(1, 0);

        cafeteria.trayDropoff.IsBusy = false;
        hasTray = true;
        isBusy = false;
        SetIdle();
    }


    protected void SitBed(GameObject bed)
    {
        navMeshAgent.destination = bed.transform.position;
        navMeshAgent.enabled = false;
        animator.SetBool("Sitting", true);
    }

    #endregion

    protected void PlaySFX(AudioClip clip)
    {
        if (clip)
        {
            audioS.clip = clip;
            audioS.Play();
        }
        else Debug.LogError("Missing sfx " + base.name);
    }

    protected void CheckClock()
    {
        hour = Clock.instance.GetClockHour();
        minute = Clock.instance.GetClockMinute();
    }

    protected virtual void SetHeader()
    {
        if (!nPCHeader) nPCHeader = transform.GetChild(0).gameObject;
        nPCHeader.name = name;
    }

    protected void SetIdle()
    {

        if (this.tag == "Patient")
        {
            Patient p = this.GetComponent<Patient>();
            if (p.patientState != Patient.PatientState.goToPlace)
            {
                p.patientState = Patient.PatientState.idle;
            }
        }
        else if (this.tag == "Guard")
        {
            this.GetComponent<Guard>().guardState = Guard.GuardState.waiting;
        }
    }

    protected void UpdateState(Patient.PatientState state)
    {
        Patient p = GetComponent<Patient>();
        p.lastpatientState = p.patientState;
        p.patientState = state;
    }

    protected void UpdateState(Guard.GuardState state)
    {
        Guard g = GetComponent<Guard>();
        g.guardState = g.lastGuardState;
        g.guardState = state;
    }
}
