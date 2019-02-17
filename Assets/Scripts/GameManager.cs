using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    private int hour, minute;

    // public List<Guard> guards = new List<Guard>();
    // public List<Patient> patients = new List<Patient>();
    private Clock clock;

    [Header("Console Debugging")]
    public bool nPCDebug;
    public bool guardDebug;
    public bool patientDebug;
    [Header("Path Debugging")]
    public bool patientPath;
    public bool guardPath;

    #region Mission Date
    string[] missionTitles = { " ", "Daily punishment", "Something funny", "Bully the bully", "Dreamy night", "Truth" };


    string[][] missionText = new string[][]
    {
        new string[] {"I woke up in this weird place...\n",
        "\n",
        "\n",
        "" },
    new string[] {"Muffled shouts of pain escape to the corridor every day.\n",
        "I have found patient 9 is daily beaten by one of the guards, disgraceful... \n",
        "The guard always goes to the toilet before he torments 9...I wonder if I can take advantage of that. \n",
        "Patient 9 wont be beaten today...I wonder what he'll say, the sink swallowed my baton?" },
    new string[] {"A few patients are constantly feeling sick, they were all complaining about the food.\n",
        "The pots, no, the kitchen itself, stinks of rot...I wonder what they put in there.\n",
        "The stuff they keep in the kitchen would make a pig sick! I wonder if they would like it themselves.\n",
        "I've mixed what I could find into the staff's pot. Lets see how they enjoy it themselves, hmm..." },
    new string[] {"bullying the bully..."},
    new string[] {"I heard some screams of terror when passing the west corridor, I think they were coming from room 17.","Some nurses are playing Freddy Krueger, I found an empty bottle of amphetamines in the room. \n A small overdose will cause big hallucinations to anyone who already has them, what a cowardly prank. \n I could get some more from the pharmacy and...hmmm" , "I grabbed a full bottle of them from the pharmacy, now I just need to find a box of mints.","I exchanged their mints for amphetamines. They'll get the kick of their lives, ha." },
    new string[] {"..."}
    };

    [Header("Missions")]
    //[HideInInspector]
    public int[] missionCheckpoints = { 0, 0, 0, 0, 0 };

    [Header("Mission Checkpoints")]
    [SerializeField]
    public GameObject[] mission0Checkpoints;
    public GameObject[] mission1Checkpoints;
    public GameObject[] mission2Checkpoints;
    public GameObject[] mission3Checkpoints;

    #endregion

    [Header("SFX")]
    public AudioClip sfx_success = null;
    public AudioClip sfx_fail = null;

    GUICheckpoint cP = null; // Checkpoint panel.
    Journal journal = null;
    public Patient[] patients;
    public List<PatientID> maleIds;
    public List<PatientID> femaleIds;



    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);

    }

    // Use this for initialization
    void Start()
    {
        clock = this.GetComponent<Clock>();
        clock.IsFrozen = false;
        hour = clock.GetClockHour();
        minute = clock.GetClockMinute();

        cP = GUICheckpoint.inst;
        cP.Toggle(false);
        journal = Journal.inst;
    }

    public void UpdatePatientIDs()
    {
        int x = 0, y = 0;
        foreach (Patient pat in patients)
        {
            if (pat.mf) // Male or Female.
            {
                Undo.RecordObject(pat, "Update id.");
                pat.id = maleIds[x];
                x++;
            }
            else
            {
                Undo.RecordObject(pat, "Update id.");
                pat.id = femaleIds[y];
                y++;
            }
            pat.name = pat.id.name;
        }
    }

    public void Checkpoint(int r)
    {
        // Display checkpoint message.
        cP.Toggle(true);
        // Play sound.
        if (sfx_success) { Player.instance.PlaySFX(sfx_success); }

        // Update Journal
        if (missionCheckpoints[r] == 0) // If it's the first checkpoint
        {
            journal.missionEntries[r].gameObject.SetActive(true); // Enable journal entry.
            journal.missionEntries[r].title = missionTitles[r];
            journal.missionEntries[r].GetComponentInChildren<Text>().text = missionTitles[r];

            cP.GetComponentInChildren<Text>().text = "New mission. Check journal for more information";

            if (r == 2)
            {
                Map.instance.kitchen.patientsPot.AddComponent<PatientsPot>();
            }
        }
        else if (missionCheckpoints[r] == 4) // If it's the last.
        {
            cP.GetComponentInChildren<Text>().text = "Routine broken..." + missionText[r];
        }
        else if (missionCheckpoints[r] > 4)
        {
            return;
        }
        else // If it's in between.
        {
            cP.GetComponentInChildren<Text>().text = "Mission Updated. Check journal for more information";
        }

        Journal.inst.missionEntries[r].text += missionText[r][missionCheckpoints[r]]; // Update/append entry text.

        Invoke("MessageTimeout", 5);
        // Update checkpoint.
        missionCheckpoints[r]++;
    }

    void MessageTimeout()
    {
        cP.Toggle(false);
    }


    void UpdateClock()
    {
        hour = clock.GetClockHour();
        minute = clock.GetClockMinute();
    }
}
