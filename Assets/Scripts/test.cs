﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("start");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnCollisionEnter (Collision collision)
    {
        if(collision.transform.gameObject.tag == "Curb")
        {
            Debug.Log("collision");
        }
    }
    
    private void OnTriggerEnter(Collider collider)
    {
        if(collider.transform.gameObject.CompareTag("Target"))
        {
            Debug.Log("Target");
        }
    }
    
    void OnCollisionExit (Collision collision)
    {
        if(collision.transform.gameObject.tag == "Curb")
        {
            Debug.Log("exit collision");
        }
    }
}
