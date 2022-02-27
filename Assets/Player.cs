using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    Vector3 tether;
    float length = 5;
    Rigidbody rb;
    bool tethered = false;
    Rigidbody hookedBody;
    float pitch = 0;
    GameObject camRig;
    GameObject crosshair;
    LineRenderer rope;
    float jumpForce = 4;
    float boost = 1;
    Animator anim;
    float health = 100;
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        rb = GetComponent<Rigidbody>();
        camRig = Camera.main.transform.parent.gameObject;
        crosshair = transform.Find("Crosshair").gameObject;
        rope = GetComponentInChildren<LineRenderer>();
        anim = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {       
        look();
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            shoot();
        }
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            shootHook();            
        }
        if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            tethered = false;
            rope.enabled = false;
        }
        if(Input.GetKey(KeyCode.Space))
        {
            jumpForce = Mathf.Clamp(jumpForce + 8 * Time.deltaTime, 4, 12);
        }
        if(Input.GetKeyUp(KeyCode.Space))
        {
            jump();
        }
        if(Input.GetKey(KeyCode.LeftShift))
        {
            useBoost();
        }
        else
        {
            if (grounded())
            {
                boost = Mathf.Clamp(boost += 1 * Time.deltaTime, 0, 1);
            }
        }
        //Debug.Log("boost: "+boost);
    }

    private void LateUpdate()
    {
        animate();
    }

    private void FixedUpdate()
    {
        if (tethered)
        {
            swing();
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView,75,4*Time.deltaTime);
        }
        else
        {
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 60, 4 * Time.deltaTime);
            if (grounded())
            {
                move();
            }
            else
            {
                airControl();
            }
        }
    }
    #region movement stuff
    void swing()
    {
        if (tethered && tether!=null)
        {
            if(grounded(transform.position + rb.velocity*Time.deltaTime))
            {
                length = Vector3.Distance(tether, transform.position) * 0.98f;
            }
            Vector3 testPos = transform.position + rb.velocity * Time.deltaTime;
            if ((testPos - tether).magnitude > length)
            {
                testPos = (testPos - tether).normalized * length + tether;
                Debug.DrawRay(transform.position, testPos, Color.green, 2);
                rb.velocity = (testPos - transform.position) / Time.deltaTime;
            }
            Debug.DrawRay(transform.position, rb.velocity, Color.red);
            rope.SetPosition(1, transform.InverseTransformPoint(tether));
            length -= Input.GetAxis("Vertical") * Time.deltaTime;
            //steer
            float strength = 3f;
            float horz = Input.GetAxis("Horizontal");
            Vector3 vel = new Vector3(horz, 0, 0).normalized;
            vel *= strength;
            vel = transform.InverseTransformDirection(vel);
            rb.AddForce(vel);
        }        
    }

    void airControl()
    {
        float strength = 3f;
        float horz = Input.GetAxis("Horizontal");
        float fwd = Input.GetAxis("Vertical");
        Vector3 vel = new Vector3(horz, 0, fwd).normalized;
        vel *= strength;
        vel = transform.rotation * vel;
        rb.AddForce(vel);
    }

    void useBoost()
    {
        boost = Mathf.Clamp(boost -= 2 * Time.deltaTime,0,1);
        rb.velocity += transform.forward * boost;
    }

    void jump()
    {
        if(grounded())
        {
            Vector3 vel = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
            rb.velocity = vel;
            jumpForce = 0;
        }
    }

    void look()
    {
        float Xmod = 1.5f; //x sensitivity
        float Ymod = 2; //x sensitivity
        float length = 1 - (Mathf.Abs(pitch) / 200);
        pitch -= Input.GetAxis("Mouse Y") * Ymod;
        pitch = Mathf.Clamp(pitch, -80, 60);
        Quaternion rot = Quaternion.Euler(0, Input.GetAxis("Mouse X") * Xmod, 0);
        transform.rotation *= rot;
        //camstick stuff
        Vector3 stick = new Vector3(0, 1.6f * length, -3.3f * length);
        //Debug.Log("length: " + length);
        stick = transform.rotation * stick;
        stick = Quaternion.AngleAxis(pitch, transform.right) * stick;
        Vector3 newPos = transform.position + transform.right;
        RaycastHit hit;
        if (Physics.Raycast(newPos, stick, out hit, stick.magnitude))
        {
            camRig.transform.position = transform.position + (hit.point - transform.position) * 0.9f;
        }
        else
        {
            camRig.transform.position = newPos + stick;
        }
        crosshair.transform.position = transform.position - stick *1.5f + transform.right;
        camRig.transform.GetChild(0).LookAt(crosshair.transform);
    }

    void move()
    {
        float gravity = 8;
        float speed = 3.5f;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed *= 2;
        }
        float horz = Input.GetAxis("Horizontal");
        float fwd = Input.GetAxis("Vertical");
        float vert = rb.velocity.y;
        if (grounded())
        {
            ////anim.ResetTrigger("land");
            ////anim.ResetTrigger("fall");
            //if (vert < -0.5f)
            //{
            //    //anim.SetTrigger("land");
            //}
            //vert = 0;
            //if (Input.GetKeyUp(KeyCode.Space))
            //{
            //    vert = jumpForce;
            //    //anim.SetTrigger("jump");
            //}
        }
        else
        {
            //anim.SetTrigger("fall");
            if (vert < 0)
            {
                vert -= gravity * 2 * Time.deltaTime; //creates a nice parabola
            }
            else
            {
                vert -= gravity * Time.deltaTime;
            }
        }
        Vector3 vec = new Vector3(horz, 0, fwd);
        vec = vec.normalized * speed;
        vec.y = vert;
        vec = transform.rotation * vec;
        rb.velocity = vec;
    }

    void shootHook()
    {
        LayerMask lm = LayerMask.GetMask("Default");
        RaycastHit hit;
        List<RaycastHit> possibles = new List<RaycastHit>();
        float searchWidth = 0.2f;
        float searchHeight = 0.2f;
        for (float y = -searchHeight; y < searchHeight; y += 0.05f)
        {
            for (float x = -searchWidth; x < searchWidth; x += 0.05f)
            {
                Ray r = Camera.main.ViewportPointToRay(new Vector3(0.5f + x, 0.5f + y, 0));
                if (Physics.Raycast(r, out hit, 50,lm))
                {
                    possibles.Add(hit);
                    Debug.DrawRay(r.origin, r.direction * 50, Color.green,2);
                    Debug.Log("Hit: " + hit.transform.name);
                    //tether = hit.point;
                    //length = Vector3.Distance(tether, transform.position);
                    //tethered = true;
                    //rope.enabled = true;
                }
                else
                {
                    Debug.DrawRay(r.origin, r.direction * 50, Color.red,2);
                }
            }
        }
        if (possibles.Count > 0)
        {
            Vector3 center = new Vector3(Screen.width / 2, Screen.height / 2, 0);
            float bestDist = Vector3.Distance(Camera.main.WorldToScreenPoint(possibles[0].point), center);
            tether = possibles[0].point;
            foreach (RaycastHit h in possibles)
            {
                float newDist = Vector3.Distance(Camera.main.WorldToScreenPoint(h.point), center);
                if (newDist < bestDist)
                {
                    bestDist = newDist;
                    tether = h.point;
                }
            }
            length = Vector3.Distance(tether, transform.position);
            tethered = true;
            rope.enabled = true;
        }
        else
        {
            tethered = false;
            rope.enabled = false;
        }
    }

    bool grounded()
    {
        Debug.DrawRay(transform.position, -Vector3.up * GetComponent<Collider>().bounds.extents.y * 1.1f, Color.red);
        if (Physics.Raycast(transform.position, -Vector3.up, GetComponent<Collider>().bounds.extents.y * 1.1f))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    bool grounded(Vector3 newPos)
    {
        Debug.DrawRay(newPos, -Vector3.up * GetComponent<Collider>().bounds.extents.y * 1.1f, Color.red);
        if (Physics.Raycast(newPos, -Vector3.up, GetComponent<Collider>().bounds.extents.y * 1.1f))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    #endregion

    void animate()
    {
        Vector3 vel = rb.velocity;
        vel = transform.InverseTransformDirection(vel);
        vel = vel.normalized;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            vel *= 2;
        }
        Debug.Log(vel);
        anim.SetFloat("horz", vel.x);
        anim.SetFloat("fwd", vel.z);
        anim.SetFloat("vert", vel.y);
        anim.SetBool("jump", !grounded()); //set jump bool to opposite of grounded result
        anim.SetBool("swing", tethered);

        if (tethered)
        {
            transform.Find("Character").rotation = Quaternion.LookRotation(transform.position + rb.velocity, tether - transform.position);
        }
        else
        {
            transform.Find("Character").rotation = transform.rotation;
        }
    }

    public void hurt(float dmg,RaycastHit hit)
    {
        health -= dmg;
        if(health<=0)
        {
            die(hit);
        }    
    }
    void die(RaycastHit deathSpot)
    {

    }

    void shoot()
    {
        RaycastHit hit;
        Ray r = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if(Physics.Raycast(r, out hit, 100))
        {
            if(hit.transform.GetComponent<Player>())
            {
                hit.transform.GetComponent<Player>().hurt(25, hit);
            }
            else if(hit.transform.GetComponent<Enemy>())
            {
                hit.transform.GetComponent<Enemy>().hurt(25, hit);
            }
        }        
    }

}
