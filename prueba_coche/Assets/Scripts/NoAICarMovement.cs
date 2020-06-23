using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoAICarMovement : MonoBehaviour
{
    public List<AxleInfo> axleInfos; // the information about each individual axle
    public GameObject path;
    private Transform[] objectives;
    public int currentObjective;
    private Rigidbody rigidBody;
    public float maxMotorTorque;
    public float maxSteeringAngle;
    private float speed;
    public Vector3 positionObjective;
    public float distance;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = gameObject.GetComponent<Rigidbody>();
        rigidBody.centerOfMass = new Vector3(0, -0.4f, 0);
        currentObjective = 1;
        objectives = path.GetComponentsInChildren<Transform>();
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

        visualWheel.transform.position = position;
        visualWheel.transform.rotation = rotation;
    }
    private void Steering(WheelCollider lw, WheelCollider rw)
    {
        // creating the relative vector between car position and objective
        Vector3 relative = transform.InverseTransformPoint(objectives[currentObjective].position);
        // dividing the relative vector between its length to obtain values between -1 and 1 and
        // multiplying this and the maxSteeringAngle to obtain the steer
        float steer = (relative.x / relative.magnitude) * maxSteeringAngle;
        lw.steerAngle = steer;
        rw.steerAngle = steer;
    }

    private void Acceleration(WheelCollider lw, WheelCollider rw)
    {
        lw.motorTorque = maxMotorTorque;
        rw.motorTorque = maxMotorTorque;
    }

    private void CheckDistance()
    {
        distance = Vector3.Distance(transform.position, objectives[currentObjective].position);
        if (Vector3.Distance(transform.position, objectives[currentObjective].position) < 1f)
        {
            currentObjective++;
            if (currentObjective >= objectives.Length)
            {
                currentObjective = 1;
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        positionObjective = objectives[currentObjective].position;
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
        speed = Mathf.Abs(rigidBody.velocity.magnitude) * 3.6f; // z axis direction speed in m/s plus conversion factor to km/h
        CheckDistance();
    }
}
