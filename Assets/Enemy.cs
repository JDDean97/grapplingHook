using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    Rigidbody rb;
    Animator anim;
    float health = 100;
    NavMeshAgent nav;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        nav = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        move();
    }

    void move()
    {

    }

    public void hurt(float dmg, RaycastHit hit)
    {
        health -= dmg;
        if (health <= 0)
        {
            die(hit);
        }
    }
    void die(RaycastHit deathSpot)
    {

    }

}
