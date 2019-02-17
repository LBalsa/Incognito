using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AdvancedCoroutines;

[SelectionBase]
public class Patient : NPC, IInteractable
{
    [Tooltip("Patient Details")]

    public PatientID id;
    public bool mf;
    bool debug;
    bool addedToJournal = false;
    bool assignReset;

    public enum PatientState
    {
        idle, waiting, talking,
        moving, washing, peeing,
        showering, running, sittingIdle,
        sittingTalking, sittingEating,
        wakeUp, sleep, goToPlace
    }
    public PatientState patientState;
    public PatientState lastpatientState;

    [System.Serializable]
    public struct Routine
    {
        public Vector2 time;
        public PatientState state;
        public Place.PlaceType goTo;
    }

    void Start()
    {
        if (!id)
        {
            return;
        }// Enable/Disable debugging.
        debug = GameManager.instance.patientDebug;
        nPCDebug = GameManager.instance.nPCDebug;
        // Check for identification.
        if (!id)
        {
            Debug.LogError(name + " does not have a patient id..");
            gameObject.SetActive(false);
        }
        patientState = PatientState.idle;
        name = id.name; SetHeader();
        // Search for own room.
        foreach (var cell in Map.instance.cells)
        {
            if (cell.cellNumber == id.patientNumber)
            { home = cell.GetComponent<Place>(); }
        }
        // Check ward wing.
        if (id.patientNumber >= 0 && id.patientNumber <= 15)
        { wing = Wings.Central; }
        else if (id.patientNumber >= 16 && id.patientNumber <= 27)
        { wing = Wings.West; }
        else if (id.patientNumber >= 28 && id.patientNumber <= 39)
        { wing = Wings.East; }
        // Setup components.
        animator = GetComponent<Animator>();
        audioS = GetComponent<AudioSource>();
        navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        navMeshAgent.autoBraking = false;
        navMeshAgent.stoppingDistance = .5f;
        navMeshAgent.Warp(home.transform.position);

        isBusy = false;

        // Start routine;
        FindRoutine();
        InvokeRepeating("CheckRoutine", 0.5f, 0.5f);

        if (Vector3.Distance(transform.position, home.transform.position) > 1)
        {
            navMeshAgent.Warp(home.transform.position);
        }
    }

    void Update()
    {
        if (!id)
        {
            return;
        }
        if (GameManager.instance.patientPath) { DrawPath(); }
        UpdateAnimator();
        switch (patientState)
        {
            case PatientState.idle:
                if (currentPlace) { IdleSwitch(); }
                break;
            case PatientState.sittingIdle:
                // Start talking.
                //if (Random.Range(0, 200) == 50)
                //{
                //    animator.SetLayerWeight(1, 1);
                //    animator.SetBool("Talking", true);
                //    UpdateState(PatientState.sittingTalking);
                //}
                break;
            case PatientState.talking:
                // Stop talking.
                if (Random.Range(0, 150) == 50)
                {
                    animator.SetBool("Talking", false);
                    UpdateState(PatientState.waiting);
                    animator.SetLayerWeight(1, 0);
                }
                break;
            case PatientState.sittingTalking:
                // Stop talking.
                if (Random.Range(0, 100) == 50)
                {
                    animator.SetLayerWeight(1, 1);
                    animator.SetBool("Talking", false);
                    UpdateState(PatientState.sittingIdle);
                    animator.SetLayerWeight(1, 0);
                }
                break;
            case PatientState.sleep:
                if (lastpatientState != patientState && !isBusy)
                {
                    StartCoroutine("GoToSleep");
                    UpdateState(patientState);
                }
                break;
            case PatientState.wakeUp:
                if (lastpatientState != patientState && !isBusy)
                {
                    StartCoroutine("WakeUp");
                    UpdateState(patientState);
                }
                break;
            case PatientState.goToPlace:
                if (lastpatientState != PatientState.goToPlace)
                { FinishPreviousRoutine(); }
                else if (!isBusy)
                {
                    GoToPlaceSwitch(place);
                    UpdateHeader(" going to " + place.placeType);
                    navMeshAgent.isStopped = false;
                    patientState = PatientState.moving;
                }
                break;
        }
    }

    void FindRoutine()
    {
        CheckClock(); // Get current time.
        int controlY = 0;
        bool success = false;
        // Check latest routines within the hour.
        foreach (Routine r in id.pr.routines)
        {
            if (hour == r.time.x)
            { // Compare hour.
                if (minute >= r.time.y)
                { // Compare minute.
                    StartCoroutine(AssignRoutineWithDelay(r, 5));
                    success = true;
                }
                else break; // Break if correct routien was found.
            }
        }
        // Check routines in past hours.
        for (int i = hour - 1; i >= 0; i--)
        {
            if (!success)
            {
                controlY = 0;
                foreach (Routine r in id.pr.routines)
                {
                    if (i == r.time.x)
                    { // Compare hour.
                        if (controlY <= r.time.y)
                        { // Compare minute.
                            controlY = (int)r.time.y;
                            StartCoroutine(AssignRoutineWithDelay(r, 5));
                            success = true;
                        }
                        else break; // Break if correct routine was found.
                    }
                }
            }
            else break;
        }
        if (debug) { if (!success) { Debug.LogError("Initial routine missing."); } }
    }

    void CheckRoutine()
    {
        CheckClock();

        foreach (Routine r in id.pr.routines)
        {
            if (hour == r.time.x && minute == r.time.y)
            {
                if (patientState != r.state && !assignReset)// && place.placeType != r.goTo)
                {
                    assignReset = true;
                    StartCoroutine(AssignRoutineWithDelay(r, 5));
                }
            }
        }
    }

    IEnumerator AssignRoutineWithDelay(Routine r, float maxDelay)
    {
        yield return new WaitForSeconds(Random.Range(0, maxDelay));
        assignReset = false;
        lastpatientState = patientState;
        patientState = r.state;
        place.placeType = r.goTo;
        StopAllCoroutines();
    }

    void FinishPreviousRoutine()
    {
        switch (lastpatientState)
        {
            case PatientState.sittingIdle:
                isBusy = true;
                StartCoroutine(Stand(true));
                lastpatientState = PatientState.goToPlace;
                break;
            case PatientState.sittingEating:
                isBusy = true;
                StartCoroutine(ExitCafeteria(currentPlace.GetComponent<Cafeteria>()));
                lastpatientState = PatientState.goToPlace;
                break;
            //case PatientState.idle:
            //    break;
            //case PatientState.waiting:
            //    break;
            //case PatientState.talking:
            //    break;
            //case PatientState.moving:
            //    break;
            //case PatientState.washing:
            //    break;
            //case PatientState.peeing:
            //    break;
            //case PatientState.showering:
            //    break;
            //case PatientState.running:
            //    break;

            // Sitting states
            case PatientState.sittingTalking:
                // Disable talking.
                animator.SetBool("Talking", false);
                animator.SetLayerWeight(1, 0);

                isBusy = true;
                StartCoroutine(Stand(true));
                lastpatientState = PatientState.goToPlace;
                break;


            //case PatientState.wakeUp:
            //    break;
            //case PatientState.sleep:
            //    break;
            //case PatientState.goToPlace:
            //    break;
            default:
                lastpatientState = PatientState.goToPlace;
                break;
        }
    }

    void GoToPlace(Place.PlaceType placeType)
    {
        if (placeType == currentPlace.placeType) return;
        lastpatientState = patientState;
        patientState = PatientState.goToPlace;
        place.placeType = placeType;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Place")
        {
            currentPlace = other.GetComponent<Place>();
            if (other.GetComponent<Place>().placeType == place.placeType)
            { ArriveAtPlace(other.GetComponent<Place>()); }
        }
    }

    protected void ArriveAtPlace(Place place)
    {
        lastpatientState = patientState;
        patientState = PatientState.idle;

        switch (place.GetComponent<Place>().placeType)
        {
            case Place.PlaceType.home:
                int r = Random.Range(0, 2);
                if (r == 0) navMeshAgent.destination = FindSeat(place).transform.position;
                else if (r == 1) navMeshAgent.destination = place.GetComponent<Cell>().bed.transform.position;

                break;
            case Place.PlaceType.hall:
                break;

            case Place.PlaceType.cafeteria:
                //patientState = PatientState.sittingEating;
                StartCoroutine(ArriveCafeteria(place.GetComponent<Cafeteria>()));
                break;

            case Place.PlaceType.bathroomCentre:
                patientState = PatientState.peeing;
                StartCoroutine(ArriveBathroom(place.GetComponent<Bathroom>()));
                break;
            case Place.PlaceType.bathroomEast:
                patientState = PatientState.peeing;
                StartCoroutine(ArriveBathroom(place.GetComponent<Bathroom>()));
                break;
            case Place.PlaceType.bathroomWest:
                patientState = PatientState.peeing;
                StartCoroutine(ArriveBathroom(place.GetComponent<Bathroom>()));
                break;
            case Place.PlaceType.shower:
                break;
            case Place.PlaceType.dayRoom:
                StartCoroutine(ArriveDayroom(place.GetComponent<DayRoom>()));
                break;
            case Place.PlaceType.therapy1:
                StartCoroutine(Sit(FindRandomSeat(place)));
                break;
            case Place.PlaceType.therapy2:
                StartCoroutine(Sit(FindRandomSeat(place)));
                break;
            case Place.PlaceType.therapy3:
                StartCoroutine(Sit(FindRandomSeat(place)));
                break;
            case Place.PlaceType.therapy4:
                StartCoroutine(Sit(FindRandomSeat(place)));
                break;
            case Place.PlaceType.therapy5:
                StartCoroutine(Sit(FindRandomSeat(place)));
                break;
            case Place.PlaceType.therapy6:
                StartCoroutine(Sit(FindRandomSeat(place)));
                break;
            default:
                break;
        }
    }


    void UpdateAnimator()
    {
        //animator.SetFloat("Forward", navMeshAgent.desiredVelocity.magnitude);
        animator.SetFloat("Forward", Vector3.Distance(navMeshAgent.velocity, new Vector3(0, 0, 0)));

        //animator.SetFloat("Forward", Vector3.Distance(navMeshAgent.desiredVelocity, new Vector3(0, 0, 0)));

        //var speed = Vector3.Distance(navMeshAgent.desiredVelocity, new Vector3(0, 0, 0));

        //_animator.SetFloat("Speed", speed);
        //_animator.speed = speed <= 0 ? 1 : speed / _baseSpeed;
    }

    void IdleSwitch()
    {
        if (!currentPlace)
        {
            return;
        }
        switch (currentPlace.placeType)
        {
            case Place.PlaceType.home:
                break;
            case Place.PlaceType.reception:
                break;
            case Place.PlaceType.hall:
                break;
            case Place.PlaceType.cafeteria:
                break;
            case Place.PlaceType.cell:
                break;
            case Place.PlaceType.bathroomCentre:
                break;
            case Place.PlaceType.bathroomWest:
                break;
            case Place.PlaceType.bathroomEast:
                break;
            case Place.PlaceType.shower:
                break;

            case Place.PlaceType.dayRoom:
                break;
            case Place.PlaceType.bed:
                break;
            case Place.PlaceType.therapy1:
                break;
            case Place.PlaceType.therapy2:
                break;
            case Place.PlaceType.therapy3:
                break;
            case Place.PlaceType.therapy4:
                break;
            case Place.PlaceType.staffRoom:
                break;
            case Place.PlaceType.staffQuarters:
                break;
            case Place.PlaceType.staffToilet:
                break;
            case Place.PlaceType.office:
                break;
            case Place.PlaceType.pharmacy:
                break;
            default:
                GoToPlace(Place.PlaceType.dayRoom);
                break;
        }

    }

    public void InteractHands()
    {
        //throw new System.NotImplementedException();
    }

    public void InteractEyes()
    {
        if (!addedToJournal)
        {
            addedToJournal = true;
            JournalEntry je = Instantiate(Journal.inst.journalEntryPrefab, Journal.inst.patientList.transform).GetComponent<JournalEntry>();
            je.Unlock(id);

            GUINote.inst.note.text = "Motive of admission: " + id.motiveOfAdmission + "\n\n" + id.description;
            GUINote.inst.Toggle();
        }
    }
}
