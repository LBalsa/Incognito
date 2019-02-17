using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "NPC/Patient")]
public class PatientID : ScriptableObject
{
    public int patientNumber;
    public string motiveOfAdmission;
    [TextArea(1,5)]
    public string description;

    [SerializeField]
    public PatientRoutine pr;
}
