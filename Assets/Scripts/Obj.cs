using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Obj : MonoBehaviour
{
    public enum ObjType { Object, Key, Note };

    [System.Serializable]
    public struct ObjectData
    {
        public string Name;
        public ObjType type;
        public int reff;

    }

    public ObjectData data;

}
