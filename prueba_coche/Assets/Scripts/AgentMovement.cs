using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentMovement : MonoBehaviour
{
    public NavMeshAgent agent;
    public int currentObjetive;
    public GameObject[] objetives;

    // Start is called before the first frame update
    void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(objetives[0].transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            currentObjetive++;
            if (currentObjetive >= objetives.Length)
            {
                currentObjetive = 0;
            }
            agent.SetDestination(objetives[currentObjetive].transform.position);
        }
    }
}
