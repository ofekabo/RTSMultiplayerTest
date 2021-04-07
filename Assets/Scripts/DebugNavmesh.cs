using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DebugNavmesh : MonoBehaviour
{
    private NavMeshAgent _agent;

    public bool velocity;

    public bool desiredVelocity;

    public bool path;
    // Start is called before the first frame update
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        
    }

    private void OnDrawGizmos()
    {
        if (_agent != null)
        {
            if (velocity)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, transform.position + _agent.velocity);
            }
            if (desiredVelocity)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, transform.position + _agent.desiredVelocity);
            }

            if (path)
            {
                Gizmos.color = Color.black;
                var agentPath = _agent.path;
                Vector3 prevCorner = transform.position;
                foreach (var corner in agentPath.corners)
                {
                    Gizmos.DrawLine(prevCorner, corner);
                    Gizmos.DrawSphere(corner, 0.1f);

                    prevCorner = corner;
                }
            }
        }
      
    }
}