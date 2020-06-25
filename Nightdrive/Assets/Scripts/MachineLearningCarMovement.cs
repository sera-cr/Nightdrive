using java.util;
using javax.xml.soap;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/**
 * Following this tutorial to learn neural networks for
 * automatic driving AI for a car
 * https://youtu.be/C6SZUU8XQQ0
 */

public class MachineLearningCarMovement : MonoBehaviour
{
    public UnityEngine.UI.Text speedUI;
    private Vector3 startPosition, startRotation;

    private Rigidbody rigidBody;
    private float speed;

    [Range(-1f, 1f)]
    public float a, t;

    public float timeSinceStart = 0f;

    [Header("Fitness")]
    public float overallFitness;
    public float distanceMultiplier = 1.4f;
    public float avgSpeedMultiplier = 0.2f;
    public float sensorMultiplier = 0.1f;

    private Vector3 lastPosition;
    private float totalDistanceTravelled;
    private float avgSpeed;

    private float aSensor, bSensor, cSensor;

    private Vector3 input;

    private void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.eulerAngles;
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.centerOfMass = new Vector3(0, -0.4f, 0);
    }

    private void Reset()
    {
        timeSinceStart = 0f;
        totalDistanceTravelled = 0f;
        avgSpeed = 0f;
        lastPosition = startPosition;
        overallFitness = 0f;
        transform.position = startPosition;
        transform.eulerAngles = startRotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Fence"))
        {
            Reset();
        }
    }

    private void CalculateFitness()
    {
        totalDistanceTravelled += Vector3.Distance(transform.position, lastPosition);
        avgSpeed = totalDistanceTravelled / timeSinceStart;

        overallFitness = (totalDistanceTravelled * distanceMultiplier) + (avgSpeed * avgSpeedMultiplier) + (((aSensor+bSensor+cSensor)/3)*sensorMultiplier);

        if (timeSinceStart > 20 && overallFitness < 40)
        {
            Reset();
        }

        if (overallFitness >= 1000)
        {
            // Saves network to a JSON
            Reset();
        }
    }

    private void InputSensors()
    {
        Vector3 a = (transform.forward + transform.right);
        Vector3 b = (transform.forward);
        Vector3 c = (transform.forward - transform.right);

        Ray r = new Ray(transform.position, a);
        RaycastHit hit;

        if (Physics.Raycast(r, out hit))
        {
            if (hit.transform.CompareTag("Fence"))
            {
                aSensor = hit.distance / 20;
                // print("A: " + aSensor);
            }
        }

        r.direction = b;

        if (Physics.Raycast(r, out hit))
        {
            if (hit.transform.CompareTag("Fence"))
            {
                aSensor = hit.distance / 20;
                // print("A: " + aSensor);
            }
        }

        r.direction = c;

        if (Physics.Raycast(r, out hit))
        {
            if (hit.transform.CompareTag("Fence"))
            {
                aSensor = hit.distance / 20;
                // print("A: " + aSensor);
            }
        }
    }

    public void MoveCar(float v, float h)
    {
        input = Vector3.Lerp(Vector3.zero, new Vector3(0, 0, v * 11.4f), 0.02f);
        input = transform.TransformDirection(input);
        transform.position += input;

        transform.eulerAngles += new Vector3(0, Mathf.Lerp(0, h * 90, 0.02f), 0);
    }

    private void UpdateText()
    {
        speedUI.text = speed.ToString("n0");
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        InputSensors();
        lastPosition = transform.position;

        // Neural network code here
        MoveCar(a, t);

        timeSinceStart += Time.deltaTime;

        CalculateFitness();

        speed = Mathf.Abs(rigidBody.velocity.magnitude) * 3.6f; // z axis direction speed in m/s plus conversion factor to km/h
        UpdateText();
        // a = 0;
        // t = 0;
    }
}
