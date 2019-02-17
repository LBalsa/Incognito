using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "NPC/PatientRoutine")]
public class PatientRoutine : ScriptableObject
{
    [SerializeField]
    public Patient.Routine[] routines;
}
