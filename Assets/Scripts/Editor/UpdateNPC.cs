//using UnityEngine;
//using System.Collections;
//using UnityEditor;

//[CustomEditor(typeof(Guard))]
//public class UpdateNPC : Editor
//{
//	public override void OnInspectorGUI()
//	{
//		DrawDefaultInspector();

//		Guard guard = (Guard)target;
//		if(GUILayout.Button("Update NPC"))
//		{
//			guard.name = guard.nPCName;
//			guard.FindWaypoints();
//		}
//	}
//}