using java.io;
using javax.xml.soap;
using javax.xml.transform;
using org.omg.CORBA;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using weka.classifiers.trees;
using weka.core;
using weka.core.converters;

public class RegressionCarMovement : MonoBehaviour
{
    public List<AxleInfo> axleInfos; // the information about each individual axle
    private Rigidbody rigidBody; // rigidbody of the car
    public float speed; // speed of the car
    [Header("Objective")]
    public GameObject path; // objectives
    private Transform[] objectives; // array of objectives
    public int currentObjective; // counter of the current objective
    public Vector3 positionObjective; // position of the current objective
    public float distance; // distance between car and the current objective
    [Header("Engine Settings")]
    public float maxValueMotorTorque;
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
    [Header("Training Settings")]
    public float actualMotorTorque; // torque with which the vehicle moves
    private float bestMotorTorque; // best torque for the car
    public float stepMotorTorque; // amount of torque does the training algorithm add to the actual torque. Should be between 10 and 60.
    public float fixedSteer; // maximum fixed steer. Should be 25 degrees
    private Vector3 startingPosition; // starting position of the car
    private Quaternion startingRotation; //starting rotation of the car
    public int iteration; // furthest node the car has to reach while training
    private bool collided; // if the car collided with a barrier
    private bool canMove; // if the car can move
    private bool reachedPoint; // if the car has reached the node it is looking for
    private M5P predictMotorTorque; // regression algorithm  which generates a regression tree
    private Instances trainingCases;
    public float time; // time the car can stay stopped
    private string STATE = "No Knowledge"; // state of the algorithm
    [Header("User Interface")]
    public UnityEngine.UI.Text speedUI;

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 20f; // to speed up the game
        rigidBody = gameObject.GetComponent<Rigidbody>();
        rigidBody.centerOfMass = new Vector3(0, -0.4f, 0);
        currentObjective = 1;
        objectives = path.GetComponentsInChildren<Transform>();
        positionObjective = objectives[currentObjective].position;
        isBraking = false;
        heightSensorPosition = 0.35f;
        frontSensorPosition = 2.2f;
        sideSensorPosition = 0.5f;
        angleSensorPosition = 30f;
        isAvoiding = false;
        avoidMultiplier = 0f;
        startingPosition = transform.position;
        startingRotation = transform.rotation;
        canMove = false;
        reachedPoint = false;
        if (STATE == "No Knowledge")
            StartCoroutine("Training"); // training starts
    }

    IEnumerator Training()
    {
        print("TRAINING");
        // reads the file with the variables and experiences
        trainingCases = new Instances(new FileReader("Assets/Training/Experiences.arff"));

        if (trainingCases.numInstances() < 30)
        {
            for (iteration = 1; iteration < objectives.Length; iteration++)
            {
                currentObjective = 1;
                for (float torque = 300; torque <= maxValueMotorTorque; torque = torque + stepMotorTorque) // planning training loop
                {
                    transform.position = startingPosition;
                    transform.rotation = startingRotation;
                    rigidBody.velocity = Vector3.zero;
                    collided = false;
                    actualMotorTorque = torque;
                    canMove = true;
                    reachedPoint = false;
                    yield return new WaitUntil(() => reachedPoint || collided);
                    canMove = false;
                    // wait untill the car reaches the point and has not collided)
                    Instance caseToLearn = new Instance(trainingCases.numAttributes());
                    caseToLearn.setDataset(trainingCases);
                    caseToLearn.setValue(0, iteration);
                    caseToLearn.setValue(1, torque);
                    caseToLearn.setValue(2, distance);
                    trainingCases.add(caseToLearn);
                }
            }
        }
        // KNOWLEDGE:
        predictMotorTorque = new M5P();
        trainingCases.setClassIndex(0);
        predictMotorTorque.buildClassifier(trainingCases);

        File exitFile = new File("Assets/Training/FinalExperiences.arff");
        if (!exitFile.exists())
        {
            System.IO.File.Create(exitFile.getAbsoluteFile().toString()).Dispose();
        }
        ArffSaver saver = new ArffSaver();
        saver.setInstances(trainingCases);
        saver.setFile(exitFile);
        saver.writeBatch();

        STATE = "With Knowledge";
        transform.position = startingPosition;
        print("FINISHED TRAINING");
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
            steer = avoidMultiplier * fixedSteer;
        }
        else
        {
            // creating the relative vector between car position and objective
            Vector3 relative = transform.InverseTransformPoint(positionObjective);
            // dividing the relative vector between its length to obtain values between -1 and 1 and
            // multiplying this and the maxSteeringAngle to obtain the steer
            steer = (relative.x / relative.magnitude) * fixedSteer;
        }
        lw.steerAngle = steer;
        rw.steerAngle = steer;
    }

    private void Acceleration(WheelCollider lw, WheelCollider rw)
    {
        float torque;
        if (!isBraking)
        {
            torque = actualMotorTorque;
        } else
        {
            torque = 0f;
        }
        lw.motorTorque = torque;
        rw.motorTorque = torque;
    }

    private void CheckDistance()
    {
        distance = Vector3.Distance(transform.position, objectives[currentObjective].position);
        if (distance < 3f)
        {
            currentObjective++;
            if (currentObjective > iteration)
            {
                reachedPoint = true;
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
                UnityEngine.Debug.DrawRay(startingPos, transform.forward * hit.distance, Color.yellow);
                isBraking = true;
            }
        }
        UnityEngine.Debug.DrawRay(startingPos, transform.forward * brakingLength, Color.yellow);
        // front left sensor
        startingPos -= transform.right * sideSensorPosition;
        if (Physics.Raycast(startingPos, transform.forward, out hit, sensorsLength))
        {
            if (hit.transform.CompareTag("Fence"))
            {
                UnityEngine.Debug.DrawRay(startingPos, transform.forward * hit.distance, Color.green);
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
                UnityEngine.Debug.DrawRay(startingPos, Quaternion.AngleAxis(-angleSensorPosition, transform.up) * transform.forward * hit.distance, Color.blue);
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
                UnityEngine.Debug.DrawRay(startingPos, transform.forward * hit.distance, Color.green);
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
                UnityEngine.Debug.DrawRay(startingPos, Quaternion.AngleAxis(angleSensorPosition, transform.up) * transform.forward * hit.distance, Color.blue);
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
                    UnityEngine.Debug.DrawRay(startingPos, transform.forward * hit.distance, Color.red);
                    isAvoiding = true;
                    isBraking = true;
                    if (hit.normal.x < 0)
                    {
                        avoidMultiplier = -1;
                    }
                    else
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
            }
            else if (speed > 60f && speed <= 65f)
            {
                brake = mediumBrakeTorque;
            }
            else if (speed > 40f && speed <= 60f)
            {
                brake = minBrakeTorque;
            }
            else
            {
                brake = 0f;
            }
        }
        else
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

    private void CheckTimePassed()
    {
        if (Mathf.Floor(speed) > 0)
        {
            time = 0f;
        } else if (Mathf.Floor(speed) == 0 && time >= 5f)
        {
            currentObjective = 1;
            collided = true;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if ((STATE == "No Knowledge") && (canMove))
        {
            Sensors();
            time += Time.deltaTime;
            positionObjective = objectives[currentObjective].position;
            distance = Vector3.Distance(transform.position, positionObjective);
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
                Braking(axleInfo.leftWheel, axleInfo.rightWheel);
                ApplyLocalPositionToVisuals(axleInfo.leftWheel);
                ApplyLocalPositionToVisuals(axleInfo.rightWheel);
            }
            CheckDistance();
            UpdateText();
            speed = Mathf.Abs(rigidBody.velocity.magnitude) * 3.6f; // z axis direction speed in m/s plus conversion factor to km/h
            CheckTimePassed();
        }
        /*if ((STATE == "With Knowledge") && (distance > 0))
        {
            distance = Vector3.Distance(transform.position, objective);
            positionObjective = objectives[currentObjective].position;
            Instance testCase = new Instance(trainingCases.numAttributes());
            testCase.setDataset(trainingCases);
            testCase.setValue(1, distance);

            bestMotorTorque = (float)predictMotorTorque.classifyInstance(testCase);
            actualMotorTorque = bestMotorTorque;
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
            //CheckDistance();
            UpdateText();
            speed = Mathf.Abs(rigidBody.velocity.magnitude) * 3.6f; // z axis direction speed in m/s plus conversion factor to km/h
        }*/
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Fence"))
        {
            currentObjective = 1;
            collided = true;
        }
    }
}
