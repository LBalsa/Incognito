using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUINote : MonoBehaviour
{
    public static GUINote inst = null;
    public Text note;
    public bool open = false;
    // Use this for initialization
    void Awake()
    {
        if (inst == null) inst = this;
        else Destroy(this);
        note = GetComponentInChildren<Text>();
    }

    public void Toggle(string text)
    {
        if (!open)
        {
            open = true;
            note.text = text;
            GetComponent<Animation>().CrossFade("NoteIn");
            //Player.instance.Pause();
            Invoke("Pause", 1);
        }
        else
        {
            open = false;
            GetComponent<Animation>().CrossFade("NoteOut");
            Player.instance.Unpause();

        }
    }

    public void Toggle()
    {
        if (!open)
        {
            open = true;
            GetComponent<Animation>().CrossFade("NoteIn");
            StartCoroutine("PauseDelay");
        }
        else
        {
            open = false;
            GetComponent<Animation>().CrossFade("NoteOut");
            StopCoroutine("PauseDelay");
            Player.instance.Unpause();
        }
    }

    IEnumerator PauseDelay()
    {
        yield return new WaitForSeconds(1);
        Player.instance.Pause();
    }

    public void Pause()
    {
        Player.instance.Pause();
    }
}
