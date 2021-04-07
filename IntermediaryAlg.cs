using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntermediaryAlg : MonoBehaviour
{
    private List<string> popPlan = new List<string>();
    private Dictionary<string, Vector3> iaPlan = new Dictionary<string, Vector3>();
    private const string CLASSROOM = "Classroom", BATHROOM = "Bathroom",
                         CAFETERIA = "Cafeteria", TABLE = "Table", OUTSIDE = "Outside";

    public Transform outsideWP, cafeteriaWP;
    public bool covidAware;
    private UnityEngine.AI.NavMeshAgent nav;
    private GameObject[] crowd;

    void Start()
    {
        // TO-DO remove this and have POP algorithm call initializeIA()
        string[] pop = {CLASSROOM, BATHROOM, CAFETERIA, TABLE, OUTSIDE};
        initializeIA(pop);
    }

    private void initializeIA(string[] pop) { // string[] pop is received from POP algorithm
        nav = GetComponent<UnityEngine.AI.NavMeshAgent>();
        // TO-DO set Crowd capsules tag to "crowd" in scene
        crowd = GameObject.FindGameObjectsWithTag("Crowd");

        foreach(string name in pop) { // convert pop array to list
            popPlan.Add(name);
        }

        foreach(string name in popPlan) { // set a Vector3 for each location
            Vector3 position = Vector3.zero;
            switch(name) { // outside and cafeteria have only one location
                case OUTSIDE:
                    position = outsideWP.position;
                    break;
                case CAFETERIA:
                    position = cafeteriaWP.position;
                    break;
            }
            iaPlan.Add(name, position);
        } 
        StartCoroutine(startIA());
    }

    private IEnumerator startIA() { // runs in parallel with RL
        foreach(KeyValuePair<string, Vector3> location in iaPlan) {
            string name = location.Key;
            Vector3 position = location.Value;

            switch(name) {
                case OUTSIDE:
                case CAFETERIA:
                    break; // outside + cafeteria location already set
                case CLASSROOM:
                    position = pickLocation(CLASSROOM);
                    break;
                case BATHROOM:
                    position = pickLocation(BATHROOM);
                    break;
                case TABLE:
                    position = pickLocation(TABLE);
                    break;
            }

            // TO-DO send location.Value to RL
            // wait until agent has reached location.Value
            nav.SetDestination(position);
            yield return new WaitUntil(hasReached);
            //print("Destination reached");
        }
        yield return null;
    }

    private bool hasReached() {
        if (!nav.pathPending) { // is a path in the process of being computed?
            if (nav.remainingDistance <= nav.stoppingDistance) // has the agent reached the target?
                return true;
        }
        return false;
    }

    private Vector3 pickLocation(string tag) {
        return covidAware ? pickByCrowd(tag) : pickByDistance(tag);
    }

    private Vector3 pickByDistance(string tag) {
        GameObject[] locations = GameObject.FindGameObjectsWithTag(tag);
        GameObject best = locations[0];
        float min = Vector3.Distance(transform.position, locations[0].transform.position);

        foreach(GameObject location in locations) {
            float dist = Vector3.Distance(transform.position, location.transform.position);
            if(dist < min) {
                min = dist;
                best = location;
            }
            //print(location + ": " + dist);
        }
        //print("BEST = " + best);
        return best.transform.position;
    }

    private Vector3 pickByCrowd(string tag) {
        GameObject[] locations = GameObject.FindGameObjectsWithTag(tag);
        GameObject best = locations[0];
        int min = 10000000; // arbitrary large int

        foreach(GameObject location in locations) {
            CapsuleCollider col = location.GetComponent<CapsuleCollider>();
            // TO-DO set the radius of each waypoint capsule to 5
            col.radius = 5;
            int numPeople = 0;

            foreach(GameObject person in crowd) {
                if(col.bounds.Contains(person.transform.position)) 
                    numPeople++;
            }

            if(numPeople < min) {
                min = numPeople;
                best = location;
            }
            //print(location + ": " + numPeople);
        }
        //print("BEST = " + best);
        return best.transform.position;
    }
}
