﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public float acceleration;
    public float steer;
    public bool brake;

    // Update is called once per frame
    void Update()
    {
        acceleration = Input.GetAxis("Vertical");
        steer = Input.GetAxis("Horizontal");
        brake = Input.GetKey(KeyCode.Space);
    }
}
