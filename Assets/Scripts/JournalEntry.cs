using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JournalEntry : MonoBehaviour
{
    [HideInInspector]
    public bool unlocked = false;

    [HideInInspector]
    public string title;
    [HideInInspector]
    public string text;

    public int index = -1;

    TextDatabase database;
    PatientID patientID = null;

    private void Start()
    {
        //index = Journal.inst.reportEntries.IndexOf(this);//System.Array.IndexOf(Journal.inst.reportEntries, this);
        //database = Journal.inst.reportDatabase;
        //if (index < 0)
        //{
        //    // index = Journal.inst.patientEntries.IndexOf(this);
        //}
        //if (index < 0)
        //{
        //    // index = Journal.inst.missionEntries.IndexOf(this);
        //}
    }

    public void Unlock(int arrayIndex, TextDatabase array)
    {
        index = arrayIndex;
        database = array;
        GetComponentInChildren<Text>().text = database.entries[index].title;
        unlocked = true;
    }

    public void Unlock(PatientID patient)
    {
        patientID = patient;
        GetComponentInChildren<Text>().text = patientID.name;
        unlocked = true;
    }

    //public void UpdateTitle(string txt)
    //{
    //    GetComponentInChildren<Text>().text = txt;
    //}

    //public void UpdateText(string txt)
    //{
    //    text = txt;
    //}

    public void OnClick()
    {
        if (unlocked)
        {
            if (patientID)
            {
                Journal.inst.UpdateText("Motive of admission: " + patientID.motiveOfAdmission + "\n\n" + patientID.description);
            }
            else if (database)
            {
                Journal.inst.UpdateText(database.entries[index].main);
            }
        }
        else
        {
            Journal.inst.UpdateText(text);
        }
        //Journal.inst.UpdateText(text);
    }

}
