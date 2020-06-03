using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputManager))]
public class CarController : MonoBehaviour
{
    public List<AxleInfo> axleInfos; // the information about each individual axle
    public float maxMotorTorque; // maximum torque the motor can apply to wheel
    public float maxSteeringAngle; // maximum steer angle the wheel can have
    private Rigidbody rigidBody;
    public InputManager im;
    public float acceleration;
    public float brake;
    public float speed;
    public Text speedUI;
    public Vector3 speedVector;

    // damper: how much does the wheel NOT bounce. With 100 damper, the wheel will bounce a lot.
    // spring: the force of the suspension spring. With 1000 spring, the spring won't have the force to recover or
    // will recover very slow.

    public void Start()
    {
        rigidBody = gameObject.GetComponent<Rigidbody>();
        rigidBody.centerOfMass = new Vector3(0, -0.4f, 0);
    }

    public void ApplyLocalPositionToVisuals(WheelCollider collider)
    {
        if (collider.transform.childCount == 0) // collider without visuals
        {
            return;
        }

        Transform visualWheel = collider.transform.GetChild(0); // getting the visual wheel from collider's child

        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);
        rotation *= Quaternion.Euler(0, 0, -90); // adjusting wheels to stay vertical

        visualWheel.transform.position = position;
        visualWheel.transform.rotation = rotation;
    }

    private void Steering(WheelCollider lw, WheelCollider rw)
    {
        // adjusting the steer to be between -1 and 1 plus the maximum steer angle
        float steer = Mathf.Clamp(im.steer, -1, 1) * maxSteeringAngle;
        lw.steerAngle = steer;
        rw.steerAngle = steer;
    }

    private void Acceleration(WheelCollider lw, WheelCollider rw)
    {
        // adjusting the acceleration to be between -1 and 1
        float accel = Mathf.Clamp(im.acceleration, -1, 1);
        // thurstTorque is the force applied in the wheel at the force app point
        // (lower point where the forces are applied)
        float thurstTorque = accel * maxMotorTorque;
        lw.motorTorque = thurstTorque;
        rw.motorTorque = thurstTorque;
    }

    private void Brake(WheelCollider lw, WheelCollider rw)
    {
        if (im.brake)
        {
            lw.brakeTorque = brake;
            rw.brakeTorque = brake;
        } else
        {
            lw.brakeTorque = 0;
            rw.brakeTorque = 0;
        }
    }

    private void UpdateText()
    {
        speedUI.text = speed.ToString("n0");
    }

    public void FixedUpdate()
    {
        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.steering)
            {
                Steering(axleInfo.leftWheel, axleInfo.rightWheel);
            }
            if (axleInfo.motor)
            {
                Acceleration(axleInfo.leftWheel, axleInfo.rightWheel);
                float force = rigidBody.mass * acceleration;
                rigidBody.AddRelativeForce(0, 0, im.acceleration * force);
            }
            Brake(axleInfo.leftWheel, axleInfo.rightWheel);
            ApplyLocalPositionToVisuals(axleInfo.leftWheel);
            ApplyLocalPositionToVisuals(axleInfo.rightWheel);
        }
        speed = Mathf.Abs(rigidBody.velocity.magnitude) * 3.6f; // z axis direction speed in m/s plus conversion factor to km/h
        UpdateText();
        speedVector = rigidBody.velocity;
    }
}

[System.Serializable]
public class AxleInfo
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor; // is this wheel attached to motor?
    public bool steering; // does this wheel apply steer angle?
}