using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(GameManager))]
public class UpdatePatientIDs : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GameManager gm = (GameManager)target;

        if (GUILayout.Button("Update Patient IDs"))
        {
            gm.UpdatePatientIDs();
        }
    }
}