using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentMovement : MonoBehaviour
{
    public NavMeshAgent agent;
    public int currentObjetive;
    public GameObject path;
    private Transform[] objectives;
    public Vector3 lastPosition;
    public GameObject car;
    private INavMeshCarMovement carObject;

    // Start is called before the first frame update
    void Start()
    {
        objectives = path.GetComponentsInChildren<Transform>();
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(objectives[1].position);
        lastPosition = agent.transform.position;
        carObject = car.GetComponent<INavMeshCarMovement>();
    }

    private void CheckSpeed()
    {
        if (carObject.isStarting)
        {
            agent.speed = 10f;
        } else
        {
            if (carObject.speed <= 10f)
            {
                agent.speed = 6f;
            }
            else if (carObject.speed > 10f && carObject.speed <= 30f)
            {
                agent.speed = 8f;
            }
            else if (carObject.speed > 30f && carObject.speed <= 40f)
            {
                agent.speed = 10f;
            }
            else if (carObject.speed > 40f && carObject.speed <= 60f)
            {
                agent.speed = 13f;
            }
            else
            {
                agent.speed = 18f;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // CheckSpeed();
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            currentObjetive++;
            if (currentObjetive >= objectives.Length)
            {
                currentObjetive = 1;
            }
            lastPosition = agent.transform.position;
            agent.SetDestination(objectives[currentObjetive].position);
        }
    }
}
