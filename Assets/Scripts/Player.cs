using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public static Player instance = null;

    public static bool paused;

    public float speedM = 5f;
    public float speedR = 150f;

    public List<Obj.ObjectData> Inventory = new List<Obj.ObjectData>();

    Animator anim = null;
    AudioSource aus = null;
    GameObject interactable = null;
    GameObject handL = null;
    GameObject handR = null;
    GameObject iP = null; // Interaction panel.
    Text iT = null; // Interaction text.

    enum PlayerState { Playing, Paused, Journal, Inventory, Dead }
    PlayerState ps = PlayerState.Playing;
    // Movement
    public float m_Damping = 0.0f;
    private readonly int m_HashHorizontalPara = Animator.StringToHash("Horizontal");
    private readonly int m_HashVerticalPara = Animator.StringToHash("Vertical");

    float d;
    void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(this.gameObject); }
        anim = GetComponent<Animator>();
        aus = GetComponent<AudioSource>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        iP = GUIInteraction.inst.gameObject;
        iT = iP.GetComponentInChildren<Text>();
        iP.SetActive(false);
        iP.GetComponent<CanvasGroup>().alpha = 1;
    }

    void Update()
    {
        if (!paused)
        {
            CheckInteractions();
            HandleInteractions();
            Movement();
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            Journal.inst.Toggle();
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            GUINote.inst.Toggle();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (Time.timeScale == 1)
            {
                Pause();
            }
            else if (Time.timeScale == 0)
            {
                Unpause();
            }
        }
    }

    void ChangeState(PlayerState pss)
    {
        if (ps != pss)
        {
            switch (ps)
            {
                case PlayerState.Playing:
                    break;
                case PlayerState.Paused:
                    break;
                case PlayerState.Journal:
                    Journal.inst.Toggle();
                    break;
                case PlayerState.Inventory:
                    break;
                case PlayerState.Dead:
                    break;
                default:
                    break;
            }
        }
        ps = pss;
        switch (ps)
        {
            case PlayerState.Playing:
                break;
            case PlayerState.Paused:
                break;
            case PlayerState.Journal:
                Journal.inst.Toggle();
                break;
            case PlayerState.Inventory:
                break;
            case PlayerState.Dead:
                break;
            default:
                break;
        }

    }

    void Movement()
    {
        float horizontal = Input.GetAxis("Horizontal") * Time.deltaTime * speedM;
        float vertical = Input.GetAxis("Vertical") * Time.deltaTime * speedM;
        // x *= Time.deltaTime;
        //z *= Time.deltaTime;
        Vector2 input = new Vector2(horizontal, vertical).normalized;
        // Run.
        if (Input.GetKey(KeyCode.LeftShift))
        {
            input.y *= 1.5f;
        }
        //anim.SetFloat("Horizontal", input.x);
        //anim.SetFloat("Vertical", input.y);
        anim.SetFloat(m_HashHorizontalPara, input.x, m_Damping, Time.deltaTime);
        anim.SetFloat(m_HashVerticalPara, input.y, m_Damping, Time.deltaTime);

        //transform.Translate(x, 0, z);
        //transform.Rotate(0, x, 0);

        anim.SetBool("Crouch", Input.GetKey(KeyCode.LeftControl));

        /*
                if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
                {
                    anim.SetBool("Crouch", !anim.GetBool("Crouch"));
                }
            */
    }
    //Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward, Color.red);

    void CheckInteractions()
    {
        RaycastHit h;
        Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out h, 1.5f);
        // Raycast for objects in front of the view.
        if (h.collider)
        {
            // Check object for IInteractible.
            if (h.collider.GetComponent<IInteractable>() != null)
            {
                // If an IInteractible is found, make it the interactible object.
                interactable = h.collider.gameObject;
                if (iP) { iP.SetActive(true); }
                iT.text = interactable.GetComponent<Obj>() ? interactable.GetComponent<Obj>().data.Name : interactable.name;
            }
            else
            {
                interactable = null;
                if (iP) { iP.SetActive(false); }
            }
        }
        else
        {
            interactable = null;
            if (iP) { iP.SetActive(false); }
        }
    }

    void HandleInteractions()
    {
        // Interact left hand.
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            if (!handL && interactable)
            {
                interactable.GetComponent<IInteractable>().InteractHands();
            }
            else if (handL)
            {
                Drop();
            }
        }
        // Interact right hand.
        if (Input.GetMouseButtonDown(1))
        {
            if (!handR && interactable)
            {
                //interactable.GetComponent<IInteractable>().InteractHands();
            }
            else if (handR)
            {
                //Drop();
            }
        }
        // Interact eyes.
        else if (Input.GetKeyDown(KeyCode.E))
        {
            if (interactable)
            {
                interactable.GetComponent<IInteractable>().InteractEyes();
            }
        }

        // Move object if carrying.
        if (handL)
        {
            Vector3 pos = handL.transform.position + Camera.main.transform.forward * d;
            handL.transform.position = pos;
        }

    }


    GameObject ReturnInteractions()
    {
        RaycastHit h;
        Physics.Raycast(transform.position, transform.forward, out h, 1.5f);
        if (h.collider && h.collider.GetComponent<MonoBehaviour>() is IInteractable)
        {


            return h.collider.gameObject;
        }
        return null;
    }

    public void Pause()
    {
        paused = true;
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        //Journal.inst.gameObject.SetActive(true);
    }

    public void Unpause()
    {
        paused = false;
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        //Journal.inst.gameObject.SetActive(false);
    }

    void InteractionText()
    {
        if (interactable)
        {
            iP.SetActive(true);
            iT.text = interactable.GetComponent<Obj>() ? interactable.GetComponent<Obj>().data.Name : interactable.name;
        }
        else
        {
            iP.SetActive(false);
        }
    }

    public void Hmm()
    {
        Debug.Log("Hmm");
    }

    public void PickUp(GameObject g)
    {
        handL = g;
        d = handL.GetComponent<Renderer>().bounds.size.magnitude;
    }
    void Drop()
    {
        if (!handL.GetComponent<Rigidbody>())
        {
            handL.AddComponent<Rigidbody>();
        }
        handL.GetComponent<Rigidbody>().useGravity = true;
        handL = null;


    }


    public void PlaySFX(AudioClip clip)
    {
        aus.loop = false;
        aus.clip = clip;
        aus.Play();
    }
}