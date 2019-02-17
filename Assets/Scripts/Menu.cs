using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{

    string[] optionsX = { "Quit", "Options", "Resume", "Start", "Load" };
    Vector3[] buttonPos = { new Vector3(0, 0, 0), new Vector3(80, 0, 0), new Vector3(80, 0, 0) };
    Vector2[] buttonSca = { };
    public int pos = 0;
    public GameObject[] buttons;
    // Use this for initialization
    void Start()
    {
        buttons[1].GetComponent<Animator>().SetTrigger("ShiftR");
        buttons[2].GetComponent<Animator>().SetTrigger("ShiftRR");
        buttons[3].GetComponent<Animator>().SetTrigger("ShiftL");
        buttons[4].GetComponent<Animator>().SetTrigger("ShiftLL");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A) && pos > -2 || Input.GetKeyDown(KeyCode.LeftArrow) && pos > -2)
        {
            pos--;
            foreach (var b in buttons)
            {
                b.GetComponent<Animator>().SetTrigger("ShiftL");
            }
        }
        if (Input.GetKeyDown(KeyCode.D) && pos < 2 || Input.GetKeyDown(KeyCode.RightArrow) && pos < 2)
        {
            pos++;
            foreach (var b in buttons)
            {
                b.GetComponent<Animator>().SetTrigger("ShiftR");
            }
        }
    }
}
