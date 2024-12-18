using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavAgent : MonoBehaviour
{
    private NavMeshAgent agent;

    void Start()
    {
        // Get and store the NavMeshAgent component attached to this game object at the start of the scene.
        agent = GetComponent<NavMeshAgent>();
    }

    //move the object to a target position.
    public void MoveTo(Vector3 targetPosition)
    {
        /// Set the destination of the NavMeshAgent to the provided target position.
        agent.SetDestination(targetPosition);
    }
}
