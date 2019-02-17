using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Journal : MonoBehaviour
{

    public static Journal inst;
    public JournalEntry[] missionEntries; //= new JournalEntry[8];


    public Text details;

    public bool open = false;
    Animation anim = null;

    public GameObject[] pages = new GameObject[3];
    public int activePage = 0;

    public GameObject missionList;
    public GameObject patientList;
    public GameObject reportList;
    [SerializeField]
    public TextDatabase reportDatabase;
    public GameObject journalEntryPrefab;

    // Use this for initialization
    void Awake()
    {
        inst = this;
        anim = GetComponent<Animation>();
    }

    public void Toggle()
    {
        if (!open)
        {
            anim.CrossFade("CanvasSlideUp");
            //Player.instance.Pause();
            StartCoroutine("PauseDelay");
            open = true;
        }
        else
        {
            anim.CrossFade("CanvasSlideDown");
            open = false;
            StopCoroutine("PauseDelay");
            Player.instance.Unpause();
        }
    }

    public void Pause()
    {
        Player.instance.Pause();
    }

    public void UpdateText(string txt)
    {
        details.text = txt;
    }

    public void PageLeft()
    {
        details.text = null;
        pages[activePage].SetActive(false);// alpha = 0;
        if (activePage == 0)
        {
            activePage = 2;
        }
        else
        {
            activePage--;
        }
        pages[activePage].SetActive(true);//alpha = 1;
    }
    public void PageRight()
    {
        details.text = null;
        pages[activePage].SetActive(false);// alpha = 0;
        if (activePage == 2)
        {
            activePage = 0;
        }
        else
        {
            activePage++;
        }
        pages[activePage].SetActive(true);//alpha = 1;
    }

    IEnumerator PauseDelay()
    {
        yield return new WaitForSeconds(1);
        Player.instance.Pause();
    }
}
