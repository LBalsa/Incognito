using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/*
 * Route 0 North route
 * Route 1
 * Route 2 South circular route
 * Route 3
 * Route 5
 * Route 6
 * Route 7 
 * Route 8
 * Route 9
 * Route 10
 * Route 11 Beating of 7
 * Route 12 Poisoned food
 * Route 13 Bully
 */

[SelectionBase]
public class Guard : NPC
{
    public int guardNumber;
    int nextWaypointNumber = 0;
    int routeAltNumber = 0;

    #region States
    bool debug;
    bool isPatrolling = false;
    public bool isAgressive = false;

    public enum GuardState
    {
        passive, guarding,
        waiting, patrolling,
        searching, chasing,
        goToPlace, changeRoute,
        sittingIdle, sittingEating,
        sittingTalking, talking
    }
    [SerializeField]
    public GuardState guardState = GuardState.passive;
    public GuardState lastGuardState;

    Transform lastSeen;

    [System.Serializable]
    public struct Routine
    {
        public Vector2 time;
        public GuardState state;
        public int route;
        public Place.PlaceType goTo;
    }
    [SerializeField]
    public Routine[] routine;
    #endregion

    GuardWaypoint currentWaypoint;
    List<GuardWaypoint> waypoints = new List<GuardWaypoint>();

    public GameObject baton;

    void Awake()
    {
        // Find all waypoints in the scene and loop through them.
        SetHeader();
        FindWaypoints();
    }

    void Start()
    {
        nPCDebug = GameManager.instance.nPCDebug;
        debug = GameManager.instance.guardDebug;
        place = new Place();
        home = Map.instance.staffquarters;

        animator = GetComponent<Animator>();
        audioS = GetComponent<AudioSource>();
        navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        navMeshAgent.autoBraking = false;
        navMeshAgent.stoppingDistance = 1f;

        hand = GetComponentInChildren<Hand>().gameObject;

        GameObject bt = Instantiate(baton, transform.position, Quaternion.identity);
        bt.transform.parent = hand.transform;
        bt.transform.localPosition = new Vector3(0, 0, 0);//baton.GetComponent<PickableObject>().HandPos;
        bt.transform.localRotation = Quaternion.Euler(baton.GetComponent<PickableObject>().HandPos);

        FindRoutine();
        InvokeRepeating("CheckRoutine", 0.5f, 0.5f);
    }


    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.guardPath) { DrawPath(); }

        // If agressive, check if player within range and field of view.
        if (Player.instance && isAgressive)
        {
            Vector3 direction = (Player.instance.transform.position - this.transform.position);
            float angle = Vector3.Angle(direction, this.transform.forward);
            if (Vector3.Distance(Player.instance.transform.position, this.transform.position) < 10 && angle < 30)
            {
                StopAllCoroutines();
                UpdateState(GuardState.chasing);
                navMeshAgent.speed = speedRun;
            }
        }

        switch (guardState)
        {
            case GuardState.passive:
                break;
            case GuardState.guarding:
                break;
            case GuardState.waiting:
                animator.SetBool("Waiting", true);
                break;
            case GuardState.patrolling:
                // Reached waypoint.
                if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 1f && isPatrolling)
                {
                    ArrivedAtWaypoint();
                }
                else if (isPatrolling == false)
                {
                    ResetBools();
                    navMeshAgent.speed = speedWalk;
                    animator.SetBool("Waiting", false);
                    if (debug) Debug.Log(name + " starting patrol");
                    isPatrolling = true;
                    NextWaypoint();
                }
                break;
            case GuardState.changeRoute:
                navMeshAgent.speed = speedWalk;
                FindWaypoints();
                isPatrolling = true;
                UpdateState(GuardState.patrolling);
                //NextWaypoint();
                break;
            case GuardState.chasing:
                RaycastHit hit;
                lastSeen = Player.instance.transform;
                // Check if player can be seen.
                Vector3 direction = lastSeen.position - transform.position;
                if (Physics.Raycast(transform.position, direction, out hit))
                {
                    // If player is lost, start searching.
                    if (hit.collider.tag != "Player")
                    {
                        UpdateState(GuardState.searching);
                    }
                }
                // Chase player.
                navMeshAgent.destination = lastSeen.position;
                break;
            case GuardState.searching:
                if (lastGuardState != GuardState.searching)
                {
                    navMeshAgent.SetDestination(lastSeen.position);
                    UpdateState(GuardState.searching);
                    break;
                }
                // Wait at player's last seen location, then resume patrol.
                if (Vector3.Distance(transform.position, lastSeen.position) < 1)
                {
                    animator.SetTrigger("Look");
                    StartCoroutine(WaitThenPatrol(5f));
                }
                break;
            case GuardState.goToPlace:
                if (lastGuardState == GuardState.sittingIdle && !isBusy)
                {
                    isBusy = true;
                    StartCoroutine(Stand(true));
                    lastGuardState = GuardState.goToPlace;
                }
                else if (lastGuardState == GuardState.sittingEating && !isBusy)
                {
                    isBusy = true;
                    StartCoroutine(ExitCafeteria(currentPlace.GetComponent<Cafeteria>()));
                    lastGuardState = GuardState.goToPlace;
                }
                else if (lastGuardState == GuardState.talking && !isBusy)
                {
                    animator.SetBool("Talking", false);
                    animator.SetLayerWeight(1, 0);
                    lastGuardState = GuardState.goToPlace;
                }
                else if (lastGuardState == GuardState.sittingTalking && !isBusy)
                {
                    animator.SetBool("Talking", false);
                    animator.SetLayerWeight(1, 0);
                    StartCoroutine(Stand(true));
                    lastGuardState = GuardState.goToPlace;
                }
                if (!isBusy)
                {
                    GoToPlaceSwitch(place);
                    nPCHeader.name = name + " going to " + place.placeType;
                    guardState = GuardState.passive;
                    Debug.DrawLine(transform.position, navMeshAgent.destination, Color.gray);
                }
                break;
        }
        UpdateAnimator();
    }

    void FindRoutine()
    {
        CheckClock();

        int controlY = 0;
        bool success = false;

        // Check latest routine within the hour.
        foreach (Routine r in routine)
        {
            if (hour == r.time.x)
            {
                if (minute >= r.time.y)
                {
                    ResetBools();
                    StartCoroutine(AssignRoutineWithDelay(r, 2));
                    success = true;
                }
                else break;
            }
        }

        if (!success)
        {
            // Check routines in past hours.
            for (int i = hour - 1; i >= 0; i--)
            {
                if (!success)
                {
                    controlY = 0;
                    foreach (Routine r in routine)
                    {
                        if (i == r.time.x)
                        {
                            if (controlY <= r.time.y)
                            {
                                controlY = (int)r.time.y;

                                ResetBools();
                                StartCoroutine(AssignRoutineWithDelay(r, 2));
                                success = true;
                            }
                            else break;
                        }
                    }
                }
                else break;
            }
        }

        /*
        // Check latest routine within the past two hours.
        if (!success)
        {
            controlY = 0;
            foreach (Routine r in routine)
            {
                if (hour - 2 == r.time.x)
                {
                    if (controlY <= r.time.y)
                    {
                        controlY = (int)r.time.y;
                        previousGuardState = guardState;
                        guardState = r.state;
                        guardNumber = r.route;
                        place.placeType = r.goTo; success = true;
                    }
                    else { break; }
                }
            }
        }
        */
        if (debug) { if (!success) { Debug.LogError("Error: Initial routine not found."); } }
    }

    void CheckRoutine()
    {
        CheckClock();
        foreach (Routine r in routine)
        {
            if (hour == r.time.x && minute == r.time.y)
            {
                lastGuardState = guardState;
                guardState = r.state;
                guardNumber = r.route;
                place.placeType = r.goTo;
            }
        }
    }

    IEnumerator AssignRoutineWithDelay(Routine r, float maxDelay)
    {
        yield return new WaitForSeconds(Random.Range(0, maxDelay));

        lastGuardState = guardState;
        guardState = r.state;
        guardNumber = r.route;
        place.placeType = r.goTo;
        StopAllCoroutines();
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

    public void FindWaypoints()
    {
        waypoints.Clear();
        GameObject[] foundWaypoints = GameObject.FindGameObjectsWithTag("GuardWaypoint");
        foreach (var waypoint in foundWaypoints)
        {
            // A waypoint may contain more than one guard script to minimsie the amoutn of objects,
            // it is necessary to iterate through them and find the right one.
            GuardWaypoint[] scripts = waypoint.GetComponents<GuardWaypoint>();
            foreach (GuardWaypoint script in scripts)
            {
                // Checking if the script ref number is the same as the gaurd ref number.
                if (script.routeNumber == guardNumber && script.waypointNumber == 0)
                {
                    // If it is, and it is also waypoint 0, add it to the list of this guard's waypoints
                    // and make it the first waypoint the guard will go to to start pratolling.
                    waypoints.Add(script);
                    currentWaypoint = script;
                }
                // Otherwise if the ref matches but it is not 0, add it to the list of waypoints.
                else if (script.routeNumber == guardNumber)
                {
                    waypoints.Add(script);
                }
            }
        }
    }

    // What to execute on arrival
    void ArrivedAtWaypoint()
    {
        switch (currentWaypoint.type)
        {
            case GuardWaypoint.waypointType.start:
                if (debug) Debug.Log(name + " arriving start");
                nextWaypointNumber = (nextWaypointNumber + 1) % waypoints.Count;
                break;
            case GuardWaypoint.waypointType.normal:
                if (debug) Debug.Log(name + " arriving normal");
                nextWaypointNumber = (nextWaypointNumber + 1) % waypoints.Count;
                break;
            case GuardWaypoint.waypointType.wait:
                if (debug) Debug.Log(name + " arriving wait");
                nextWaypointNumber = (nextWaypointNumber + 1) % waypoints.Count;
                break;
            case GuardWaypoint.waypointType.shiftBack:
                if (debug) Debug.Log(name + " arriving shift back");
                routeAltNumber = 0;
                if (currentWaypoint.shiftBackTo != 0)
                {
                    nextWaypointNumber = currentWaypoint.shiftBackTo;
                }
                nextWaypointNumber = (nextWaypointNumber + 1) % waypoints.Count;
                break;
            case GuardWaypoint.waypointType.end:
                if (debug) Debug.Log(name + " arriving end");
                nextWaypointNumber = 0;
                routeAltNumber = 0;
                break;
            default:
                nextWaypointNumber = (nextWaypointNumber + 1) % waypoints.Count;
                break;
        }

        waitTime = currentWaypoint.waitTime;
        if (waitTime > 0)
        {
            StartCoroutine(Wait());
        }
        else
            NextWaypoint();
    }

    // What to execute before departure.
    void NextWaypoint()
    {
        // Shift route.
        if (currentWaypoint.type == GuardWaypoint.waypointType.shift)
        {
            var altWaypoints = new List<GuardWaypoint>();
            foreach (var waypoint in waypoints)
            {
                if (waypoint.waypointNumber == nextWaypointNumber)
                { altWaypoints.Add(waypoint); }
            }
            currentWaypoint = altWaypoints[Random.Range(0, altWaypoints.Count)];
            routeAltNumber = currentWaypoint.routeAltNumber;
            navMeshAgent.destination = currentWaypoint.transform.position;
            return;
        }
        // Skip to...
        if (currentWaypoint.type == GuardWaypoint.waypointType.skipTo)
        {
            currentWaypoint = currentWaypoint.skipRef;
            nextWaypointNumber = currentWaypoint.waypointNumber + 1;
            routeAltNumber = currentWaypoint.routeAltNumber;
            navMeshAgent.destination = currentWaypoint.transform.position;
            return;
        }
        // Normal procedure.
        foreach (var waypoint in waypoints)
        {
            if (waypoint.waypointNumber == nextWaypointNumber)
            {
                if (waypoint.routeAltNumber == routeAltNumber)
                {
                    if (debug) Debug.Log(name + " set " + waypoint.type);

                    currentWaypoint = waypoint;
                    navMeshAgent.destination = currentWaypoint.transform.position;
                    return;
                }
            }
        }
        // Failure, skip and retry.
        Debug.LogError("Missing waypoint on " + guardNumber);
        nextWaypointNumber = 0;
        NextWaypoint();
    }

    override protected void DrawPath()
    {
        for (int i = 0; i < navMeshAgent.path.corners.Length - 1; i++)
        {
            Debug.DrawLine(navMeshAgent.path.corners[i], navMeshAgent.path.corners[i + 1], Color.blue);
        }
    }


    IEnumerator Wait()
    {
        // Change guardstate to waiting so it does not keep calling ArrivedAtWaypoint.
        guardState = GuardState.waiting;
        isPatrolling = false;
        // Wait for waitTime seconds before choosing next waypoint.
        yield return new WaitForSeconds(waitTime);

        //NextWaypoint();
        guardState = GuardState.patrolling;
    }

    IEnumerator WaitThenPatrol(float seconds)
    {
        // Change guardstate to waiting.
        guardState = GuardState.waiting;
        // Wait for waitTime seconds before choosing next waypoint.
        yield return new WaitForSeconds(seconds);
        // Resume patrolling.
        guardState = GuardState.patrolling;
    }

    void ResetBools()
    {
        isPatrolling = false;
        isAgressive = false;
    }

    protected override void SetHeader()
    {
        if (!nPCHeader) nPCHeader = transform.GetChild(0).gameObject;
        nPCHeader.name = name + " " + guardNumber.ToString() + " " + guardState;
    }


    private void OnTriggerEnter(Collider other)
    {
        MissionTriggerNPC tt = other.GetComponent<MissionTriggerNPC>();
        if (tt && tt.onoff && guardState == GuardState.patrolling) // If there is an active trigger, call selector.
        {
            TaskSelector(tt);
        }

        if (guardState == GuardState.passive && other.tag == "Place")
        {
            currentPlace = other.GetComponent<Place>();

            if (currentPlace.placeType == place.placeType)
            {
                //ArriveAtPlace(other.GetComponent<Place>());
            }
        }
    }

    protected void ArriveAtPlace(Place place)
    {
        lastGuardState = guardState;
        guardState = GuardState.passive;

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
                StartCoroutine(ArriveBathroom(place.GetComponent<Bathroom>()));
                break;
            case Place.PlaceType.bathroomEast:
                StartCoroutine(ArriveBathroom(place.GetComponent<Bathroom>()));
                break;
            case Place.PlaceType.bathroomWest:
                StartCoroutine(ArriveBathroom(place.GetComponent<Bathroom>()));
                break;
            case Place.PlaceType.shower:
                break;
            case Place.PlaceType.dayRoom:
                StartCoroutine(ArriveDayroom(place.GetComponent<DayRoom>()));
                break;
            case Place.PlaceType.therapy1:
                navMeshAgent.destination = FindSeat(place).transform.position;
                break;
            case Place.PlaceType.therapy2:
                navMeshAgent.destination = FindSeat(place).transform.position;
                break;
            case Place.PlaceType.therapy3:
                navMeshAgent.destination = FindSeat(place).transform.position;
                break;
            case Place.PlaceType.therapy4:
                navMeshAgent.destination = FindSeat(place).transform.position;
                break;
            default:
                break;
        }
    }


    #region Mission Events

    void TaskSelector(MissionTriggerNPC tt)
    {
        // Select right task for guard.
        UpdateHeader("Mission trigger");
        //int taskNumber = tt.taskNumber+11;
        switch (tt.taskNumber)
        {
            case 11:
                if (guardNumber == tt.taskNumber)
                {
                    StartCoroutine(Task0(tt));
                }
                break;
            case 12:
                if (guardNumber == tt.taskNumber)
                {
                    StartCoroutine(Task1(tt));
                }
                break;
            case 13:
                if (guardNumber == tt.taskNumber)
                {
                    StartCoroutine(Task3(tt));
                }
                break;
            case 14:
                if (guardNumber == tt.taskNumber)
                {
                    StartCoroutine(Task3(tt));
                }
                break;
            case 15:
                if (guardNumber == tt.taskNumber)
                {
                    StartCoroutine(Task4(tt));
                }

                break;
            default:
                break;
        }
    }

    IEnumerator Task0(MissionTriggerNPC tt)
    {
        UpdateState(GuardState.guarding);
        tt.onoff = false;

        // Third trigger
        if (GameManager.instance.missionCheckpoints[1] == 2)
        {
            GameManager.instance.mission0Checkpoints[2].SetActive(true);
        }

        // Go to the sink.
        Waitpoint sink = FindRandomWaitpoint(Map.instance.bathroomCentre.sinks, true);
        yield return StartCoroutine(WaitpointQueue(sink));

        // Drop baton in sink.
        yield return StartCoroutine("UseHands");
        PlaySFX(fX.tap);
        GameObject bt = null;
        if (GameManager.instance.missionCheckpoints[1] == 2)
        {
            bt = Instantiate(baton, sink.transform.position + new Vector3(-0.5f, 0, 0), Quaternion.Euler(90, 0, 0));
        }
        sink.IsBusy = false;
        isBusy = false;

        // Go to a toilet.
        Waitpoint toilet = FindRandomWaitpoint(Map.instance.bathroomCentre.toilets, true);
        yield return StartCoroutine(WaitpointQueue(toilet));
        StartCoroutine("UseHands");

        // Delay so player can retrieve baton.
        yield return sec5;
        yield return sec5;
        toilet.IsBusy = false;
        PlaySFX(fX.pee);

        // Retrieve baton from sink.
        yield return StartCoroutine(WaitpointQueue(sink));
        PlaySFX(fX.tap);
        yield return StartCoroutine("UseHands");
        sink.IsBusy = false;

        // If player retrieved baton.
        if (GameManager.instance.missionCheckpoints[1] == 3 && !bt)
        {
            PlaySFX(fX.whatTheHell);
            yield return sec2;

            // Routine broken!
            GameManager.instance.Checkpoint(0);

            // Look for baton.
            navMeshAgent.SetDestination(toilet.transform.position);
            yield return sec3;
            navMeshAgent.SetDestination(toilet.transform.position);
            yield return sec3;

            // Move on
            UpdateState(GuardState.patrolling);
            yield break;
        }

        GameManager.instance.mission0Checkpoints[2].SetActive(false);

        // Go to cell.
        Vector3 target = Map.instance.cells[7].door.transform.position;
        navMeshAgent.SetDestination(target);
        yield return new WaitUntil(() => Vector3.Distance(transform.position, target) <= 2);

        // Open door.
        Map.instance.cells[7].door.InteractHands();
        yield return sec2;

        target = Map.instance.cells[7].transform.position;
        navMeshAgent.SetDestination(target);
        yield return new WaitUntil(() => Vector3.Distance(transform.position, target) <= 1);

        // Lock door.
        Map.instance.cells[7].door.InteractHands();
        Map.instance.cells[7].door.isLocked = true;

        // First trigger.
        if (GameManager.instance.missionCheckpoints[1] == 0)
        {
            GameManager.instance.mission0Checkpoints[0].SetActive(true);
        }

        //// Beating event for 1min.
        //for (int i = 0; i < 12; i++)
        //{
        //    audioS.clip = fX.beating;
        //    audioS.loop = true;
        //    audioS.Play();
        //    //PlaySFX(fX.beating);
        //    yield return sec5;
        //}
        //audioS.loop = false;

        yield return sec5;

        UpdateState(GuardState.patrolling);
        tt.onoff = true;

        if (GameManager.instance.missionCheckpoints[1] == 0)
        {
            GameManager.instance.mission0Checkpoints[0].SetActive(false);
        }

        // Second trigger
        if (GameManager.instance.missionCheckpoints[1] == 1)
        {
            GameManager.instance.mission0Checkpoints[1].SetActive(true);
            yield return sec5;
            GameManager.instance.mission0Checkpoints[1].SetActive(false);
        }
    }
    IEnumerator Task1(MissionTriggerNPC tt)
    {
        UpdateState(GuardState.guarding);
        tt.onoff = false;


        Vector3 target = Map.instance.kitchen.transform.position;
        navMeshAgent.SetDestination(target);
        yield return new WaitUntil(() => Vector3.Distance(transform.position, target) <= 1);

        // Third trigger
        if (GameManager.instance.missionCheckpoints[1] == 2)
        {
            GameManager.instance.mission0Checkpoints[2].SetActive(true);
        }

        // Second trigger
        if (GameManager.instance.missionCheckpoints[1] == 0)
        {
            GameManager.instance.mission1Checkpoints[0].SetActive(true);
            yield return sec5;
            GameManager.instance.mission1Checkpoints[0].SetActive(false);

        }
    }
    IEnumerator Task3(MissionTriggerNPC tt)
    {
        UpdateState(GuardState.guarding);
        tt.onoff = false;

        // Go to pharmacy.
        Vector3 target = Map.instance.pharmacy.transform.position;
        navMeshAgent.SetDestination(target);
        yield return new WaitUntil(() => Vector3.Distance(transform.position, target) <= 1);

        // Go to room 17.
        foreach (Cell c in Map.instance.cells)
        {
            if (c.cellNumber == 17)
            {
                target = c.transform.position;
                break;
            }
        }
        navMeshAgent.SetDestination(target);
        yield return new WaitUntil(() => Vector3.Distance(transform.position, target) <= 1);

        // Trigger on
        if (GameManager.instance.missionCheckpoints[3] == 0)
        {
            GameManager.instance.mission3Checkpoints[0].SetActive(true);
        }

        // Play sound.
        audioS.loop = true;
        PlaySFX(fX.sreams);

        yield return new WaitUntil(() => Clock.instance.hour >= 4);
        audioS.loop = false;

        // Trigger off.
        GameManager.instance.mission3Checkpoints[0].SetActive(false);

        // Check if player detected.
        if (GameManager.instance.missionCheckpoints[3] >= 0)
        {
            GameManager.instance.mission3Checkpoints[1].SetActive(true);
            yield break;
        }
        tt.onoff = true;
    }
    IEnumerator Task4(MissionTriggerNPC tt)
    {
        yield break;

    }
    IEnumerator Task5(MissionTriggerNPC tt)
    {
        yield break;

    }

    #endregion
}

/* var hit : RaycastHit;
 var rayDirection = player.position - transform.position;
 if (Physics.Raycast (transform.position, rayDirection, hit)) {
     if (hit.transform == player) {
         // enemy can see the player!
     } else {
         // there is something obstructing the view
     }
 }


    Vector3 targetDir = player.position - transform.position;
 float angleToPlayer = (Vector3.Angle(targetDir, transform.forward));
 
 if (angleToPlayer >= -90 && angleToPlayer <= 90) // 180° FOV
       Debug.Log("Player in sight!");
       */
