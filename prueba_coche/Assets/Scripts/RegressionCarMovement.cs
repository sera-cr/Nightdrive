using java.io;
using javax.xml.soap;
using javax.xml.transform;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using weka.classifiers.trees;
using weka.core;
using weka.core.converters;

public class RegressionCarMovement : MonoBehaviour
{
    public List<AxleInfo> axleInfos; // the information about each individual axle
    private Rigidbody rigidBody;
    public float speed;
    [Header("Objective")]
    public GameObject path;
    private Transform[] objectives;
    public int currentObjective;
    public Transform point;
    private Vector3 objective; // training objective
    public Vector3 positionObjective;
    public float distance;
    [Header("Engine Settings")]
    public float maxValueMotorTorque;
    [Header("Training")]
    private M5P predictMotorTorque;
    private Instances trainingCases;
    private string STATE = "No Knowledge";
    public float actualMotorTorque;
    private float bestMotorTorque;
    public float stepMotorTorque;
    public float fixedSteer;
    private Vector3 startingPosition;
    private bool collided;
    private bool canMove;
    [Header("User Interface")]
    public UnityEngine.UI.Text speedUI;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = gameObject.GetComponent<Rigidbody>();
        rigidBody.centerOfMass = new Vector3(0, -0.4f, 0);
        objective = point.position;
        startingPosition = transform.position;
        currentObjective = 1;
        objectives = path.GetComponentsInChildren<Transform>();
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
            for (float torque = 300; torque <= maxValueMotorTorque; torque = torque + stepMotorTorque) // planning training loop
            {
                canMove = false;
                transform.position = startingPosition;
                collided = false;
                actualMotorTorque = torque;
                canMove = true;
                yield return new WaitUntil(() => (distance <= 1f) || collided);
                // wait untill the car reaches the point and has not collided)
                Instance caseToLearn = new Instance(trainingCases.numAttributes());
                caseToLearn.setDataset(trainingCases);
                caseToLearn.setValue(0, torque);
                caseToLearn.setValue(1, distance);
                trainingCases.add(caseToLearn);
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
        Vector3 relative = transform.InverseTransformPoint(objective);
        // dividing the relative vector between its length to obtain values between -1 and 1 and
        // multiplying this and the maxSteeringAngle to obtain the steer
        float steer = (relative.x / relative.magnitude) * fixedSteer;

        lw.steerAngle = steer;
        rw.steerAngle = steer;
    }

    private void Acceleration(WheelCollider lw, WheelCollider rw)
    {
        lw.motorTorque = actualMotorTorque;
        rw.motorTorque = actualMotorTorque;
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

    private void UpdateText()
    {
        speedUI.text = speed.ToString("n0");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if ((STATE == "No Knowledge") && (canMove))
        {
            distance = Vector3.Distance(transform.position, objective);
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
        }
        if ((STATE == "With Knowledge") && (distance > 0))
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
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Fence"))
        {
            collided = true;
        }
    }
}
