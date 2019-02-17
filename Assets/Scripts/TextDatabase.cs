using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Text Database")]
public class TextDatabase : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public string title;
        public string subtitle;
        [TextArea()]
        public string main;
    }
    public Entry[] entries;
}
