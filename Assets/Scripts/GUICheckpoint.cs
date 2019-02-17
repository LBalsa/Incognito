using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GUICheckpoint : MonoBehaviour
{
    public static GUICheckpoint inst;
    CanvasGroup canvasGroup;

    void Awake()
    {
        if (inst == null) inst = this;
        else Destroy(this);

        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Toggle(bool on)
    {
        if (on)
        {
            canvasGroup.alpha = 1;
        }
        else
        {
            canvasGroup.alpha = 0;
        }
    }
}
