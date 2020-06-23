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

    // Start is called before the first frame update
    void Start()
    {
        objectives = path.GetComponentsInChildren<Transform>();
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(objectives[1].position);
    }

    // Update is called once per frame
    void Update()
    {
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            currentObjetive++;
            if (currentObjetive >= objectives.Length)
            {
                currentObjetive = 1;
            }
            agent.SetDestination(objectives[currentObjetive].position);
        }
    }
}
