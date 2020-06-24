using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;
using UnityEngine.UI;

public class NoAICarMovement : MonoBehaviour
{
    public List<AxleInfo> axleInfos; // the information about each individual axle
    private Rigidbody rigidBody;
    public float speed;
    [Header("Objective")]
    public GameObject path;
    private Transform[] objectives;
    public int currentObjective;
    public Vector3 positionObjective;
    public float distance;
    [Header("Engine Settings")]
    public float maxMotorTorque;
    public float mediumMotorTorque;
    public float minMotorTorque;
    public float maxSteeringAngle;
    public float maxBrakeTorque;
    public float mediumBrakeTorque;
    public float minBrakeTorque;
    public bool isBraking;
    [Header("Sensors")]
    public float sensorsLength; // should be between 6f and 10f
    public float brakingLength; // should be between 10f and 15f
    private float heightSensorPosition; // to move up the sensors
    private float frontSensorPosition; // to move the sensor to the front of the car
    private float sideSensorPosition; // to move the side sensors to the front sides of the car
    private float angleSensorPosition; // to turn angle sensors 30 degrees
    public bool isAvoiding; // if the car is avoiding obstacles
    private float avoidMultiplier; // negative = right side, positive = left side 
    [Header("User Interface")]
    public Text speedUI;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = gameObject.GetComponent<Rigidbody>();
        rigidBody.centerOfMass = new Vector3(0, -0.4f, 0);
        currentObjective = 1;
        objectives = path.GetComponentsInChildren<Transform>();
        isBraking = false;
        heightSensorPosition = 0.35f;
        frontSensorPosition = 2.2f;
        sideSensorPosition = 0.5f;
        angleSensorPosition = 30f;
        isAvoiding = false;
        avoidMultiplier = 0f;
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
        float steer;
        if (isAvoiding)
        {
            steer = avoidMultiplier * maxSteeringAngle;
        } else
        {
            // creating the relative vector between car position and objective
            Vector3 relative = transform.InverseTransformPoint(objectives[currentObjective].position);
            // dividing the relative vector between its length to obtain values between -1 and 1 and
            // multiplying this and the maxSteeringAngle to obtain the steer
            steer = (relative.x / relative.magnitude) * maxSteeringAngle;
        }
        lw.steerAngle = steer;
        rw.steerAngle = steer;
    }

    private void Acceleration(WheelCollider lw, WheelCollider rw)
    {
        if (!isBraking)
        {
            float torque;
            if (speed <= 40f)
            {
                torque = maxMotorTorque;
            }
            else if (speed > 40f && speed <= 60f)
            {
                torque = mediumMotorTorque;
            }
            else if (speed > 60f && speed <= 65f)
            {
                torque = minMotorTorque;
            }
            else
            {
                torque = 0f;
            }
            lw.motorTorque = torque;
            rw.motorTorque = torque;
        }
    }

    private void CheckDistance()
    {
        distance = Vector3.Distance(transform.position, objectives[currentObjective].position);
        if (distance < 3f)
        {
            currentObjective++;
            if (currentObjective >= objectives.Length)
            {
                currentObjective = 1;
            }
        }
    }

    private void Sensors()
    {
        RaycastHit hit;
        Vector3 startingPos = transform.position;
        startingPos += transform.forward * frontSensorPosition;
        startingPos += transform.up * heightSensorPosition;

        avoidMultiplier = 0f;
        isAvoiding = false;
        isBraking = false;

        // braking sensor
        if (speed >= 70f)
        {
            isBraking = true;
        }
        if (Physics.Raycast(startingPos, transform.forward, out hit, brakingLength))
        {
            if (hit.transform.CompareTag("Fence"))
            {
                Debug.DrawRay(startingPos, transform.forward * hit.distance, Color.yellow);
                isBraking = true;
            }
        }
        Debug.DrawRay(startingPos, transform.forward * brakingLength, Color.yellow);
        // front left sensor
        startingPos -= transform.right * sideSensorPosition;
        if (Physics.Raycast(startingPos, transform.forward, out hit, sensorsLength))
        {
            if (hit.transform.CompareTag("Fence"))
            {
                Debug.DrawRay(startingPos, transform.forward * hit.distance, Color.green);
                isAvoiding = true;
                isBraking = true;
                avoidMultiplier += 1f;
            }
        }
        // Debug.DrawRay(startingPos, transform.forward * sensorsLength, Color.green);
        // front left angle sensor
        if (Physics.Raycast(startingPos, Quaternion.AngleAxis(-angleSensorPosition, transform.up) * transform.forward, out hit, sensorsLength))
        {
            if (hit.transform.CompareTag("Fence"))
            {
                Debug.DrawRay(startingPos, Quaternion.AngleAxis(-angleSensorPosition, transform.up) * transform.forward * hit.distance, Color.blue);
                isAvoiding = true;
                isBraking = true;
                avoidMultiplier += 0.5f;
            }
        }
        // Debug.DrawRay(startingPos, Quaternion.AngleAxis(-angleSensorPosition, transform.up) * transform.forward * sensorsLength, Color.blue);
        // front right sensor
        startingPos += 2 * transform.right * sideSensorPosition;
        if (Physics.Raycast(startingPos, transform.forward, out hit, sensorsLength))
        {
            if (hit.transform.CompareTag("Fence"))
            {
                Debug.DrawRay(startingPos, transform.forward * hit.distance, Color.green);
                isAvoiding = true;
                isBraking = true;
                avoidMultiplier -= 1f;
            }
        }
        // Debug.DrawRay(startingPos, transform.forward * sensorsLength, Color.green);
        // front right angle sensor
        if (Physics.Raycast(startingPos, Quaternion.AngleAxis(angleSensorPosition, transform.up) * transform.forward, out hit, sensorsLength))
        {
            if (hit.transform.CompareTag("Fence"))
            {
                Debug.DrawRay(startingPos, Quaternion.AngleAxis(angleSensorPosition, transform.up) * transform.forward * hit.distance, Color.blue);
                isAvoiding = true;
                isBraking = true;
                avoidMultiplier -= 0.5f;
            }
        }
        // Debug.DrawRay(startingPos, Quaternion.AngleAxis(angleSensorPosition, transform.up) * transform.forward * sensorsLength, Color.blue);
        // front center sensor
        if (avoidMultiplier == 0)
        {
            if (Physics.Raycast(startingPos, transform.forward, out hit, sensorsLength))
            {
                if (hit.transform.CompareTag("Fence"))
                {
                    Debug.DrawRay(startingPos, transform.forward * hit.distance, Color.red);
                    isAvoiding = true;
                    isBraking = true;
                    if (hit.normal.x < 0)
                    {
                        avoidMultiplier = -1;
                    } else
                    {
                        avoidMultiplier = 1;
                    }
                }
            }
        }
        // Debug.DrawRay(startingPos, transform.forward * sensorsLength, Color.red);
    }

    private void Braking(WheelCollider lw, WheelCollider rw)
    {
        float brake;
        if (isBraking)
        {
            if (speed > 65f)
            {
                brake = maxBrakeTorque;
            } else if (speed > 60f && speed <= 65f)
            {
                brake = mediumBrakeTorque;
            } else if (speed > 40f && speed <= 60f)
            {
                brake = minBrakeTorque;
            } else
            {
                brake = 0f;
            }
        } else
        {
            brake = 0f;
        }

        lw.brakeTorque = brake;
        rw.brakeTorque = brake;
    }

    private void UpdateText()
    {
        speedUI.text = speed.ToString("n0");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Sensors();
        positionObjective = objectives[currentObjective].position;
        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.steering)
            {
                Sensors();
                Steering(axleInfo.leftWheel, axleInfo.rightWheel);
            }
            if (axleInfo.motor)
            {
                Acceleration(axleInfo.leftWheel, axleInfo.rightWheel);
            }
            Braking(axleInfo.leftWheel, axleInfo.rightWheel);
            ApplyLocalPositionToVisuals(axleInfo.leftWheel);
            ApplyLocalPositionToVisuals(axleInfo.rightWheel);
        }
        CheckDistance();
        UpdateText();
        speed = Mathf.Abs(rigidBody.velocity.magnitude) * 3.6f; // z axis direction speed in m/s plus conversion factor to km/h

    }
}
