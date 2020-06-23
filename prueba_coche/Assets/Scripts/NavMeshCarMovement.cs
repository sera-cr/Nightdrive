using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class NavMeshCarMovement : MonoBehaviour
{
    public List<AxleInfo> axleInfos; // the information about each individual axle
    public float minMotorTorque; // minimum torque the motor can apply to wheel
    public float maxMotorTorque; // maximum torque the motor can apply to wheel
    public float maxSteeringAngle; // maximum steer angle the wheel can have
    public float acceleration;
    private Rigidbody rb;
    public float distance;
    public GameObject agent;
    private float maximumDistance;
    public bool starting;
    public float speed;
    // public int potencia [version with only forces]

    // Start is called before the first frame update
    void Start()
    {
        starting = true;
        rb = gameObject.GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -0.4f, 0);
        maximumDistance = 1f;
        // potencia = 50; [version with only forces]
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
        Quaternion angle = agent.transform.rotation * Quaternion.Inverse(transform.rotation);
        float steer = Mathf.Clamp(angle.y, -1, 1) * maxSteeringAngle;
        lw.steerAngle = steer;
        rw.steerAngle = steer;
    }

    private void Acceleration(WheelCollider lw, WheelCollider rw)
    {
        float torque;
        if (speed >= 40f)
        {
            torque = minMotorTorque;
        }
        else
        {
            torque = maxMotorTorque;
        }
        if (starting)
        {
            torque = 200;
        }

        if (distance >= maximumDistance)
        {
            lw.motorTorque = torque;
            rw.motorTorque = torque;
            if (speed <= 36f)
            {
                float force = rb.mass * acceleration;
                float adjustedForce = (distance - 1) * force;
                rb.AddRelativeForce(0, 0, adjustedForce);
            }
        }
    }

    private void Starting()
    {
        if (starting && speed >= 35f)
        {
            starting = false;
        }
        if (!starting)
        {
            maximumDistance = 10f;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        /* [version with only forces]
         * transform.LookAt(agente.transform);
         * float distancia = Vector3.Distance(agente.transform.position, transform.position);
         * Vector3 k = Vector3.forward * potencia;
         * Vector3 fuerza = (distancia - 1) * k;
         * rigidbody.AddRelativeForce(fuerza);
         */
        Starting();
        distance = Vector3.Distance(agent.transform.position, transform.position);
        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.steering)
            {
                Steering(axleInfo.leftWheel, axleInfo.rightWheel);
            }
            if (axleInfo.motor)
            {
                Acceleration(axleInfo.leftWheel, axleInfo.rightWheel);
            }
            ApplyLocalPositionToVisuals(axleInfo.leftWheel);
            ApplyLocalPositionToVisuals(axleInfo.rightWheel);
        }
        speed = Mathf.Abs(rb.velocity.magnitude) * 3.6f; // z axis direction speed in m/s plus conversion factor to km/h
    }
}