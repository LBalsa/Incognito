using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waitpoint : MonoBehaviour
{
    private bool isBusy;
    [Tooltip("Wait time in seconds")]
    public float waitTime;
    // Current using npc.
    public GameObject current;
    // Queue of npcs.
    public int queueLenght;
    public Queue<GameObject> queue = new Queue<GameObject>();
    // Type of wait point.
    public enum Type { Caf, sink, toilet, shower };
    public Type type;
    // Optional place to start queue at.
    public Transform queuePoint;

    public bool IsBusy
    {
        get
        {
            return isBusy;
        }

        set
        {
            if (value != false)
            {
                StartCoroutine("Reset");
            }
            else
            {
                StopCoroutine("Reset");
            }
            isBusy = value;
        }
    }

    IEnumerator Reset()
    {
        if (type == Type.Caf)
        {
            yield return new WaitForSeconds(10);
            IsBusy = false;
        }
    }
}
