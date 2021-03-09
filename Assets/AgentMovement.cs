using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentMovement : MonoBehaviour
{
    public Transform outsideWP, cafeteriaWP, classroomWP, bathroomWP;
    private UnityEngine.AI.NavMeshAgent nav;

    // Start is called before the first frame update
    void Start()
    {
        nav = GetComponent<UnityEngine.AI.NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        // example: sets and moves the agent towards the destination
        nav.SetDestination(classroomWP.position);
    }
}
