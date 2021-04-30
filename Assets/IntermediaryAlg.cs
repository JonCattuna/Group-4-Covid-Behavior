using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntermediaryAlg : MonoBehaviour
{
    private List<string> popPlan;
    private const string CLASSROOM = "Classroom", BATHROOM = "Bathroom",
                         CAFETERIA = "Cafeteria", TABLE = "Table", OUTSIDE = "Outside";
    public Transform outsideWP, cafeteriaWP;
    public bool covidAware;
    private UnityEngine.AI.NavMeshAgent nav;
    private GameObject[] crowd;

    List<GameObject> visited = new List<GameObject>();

    void Start()
    {
        //popPlan = new string[5] {CLASSROOM, BATHROOM, CAFETERIA, TABLE, OUTSIDE};
        popPlan = POP.popAlgo(covidAware);
        initializeIA(popPlan);
    }

    public void initializeIA(List<string> pop) { // string[] pop is received from POP algorithm
        nav = GetComponent<UnityEngine.AI.NavMeshAgent>();
        // TO-DO set Crowd capsules tag to "crowd" in scene
        crowd = GameObject.FindGameObjectsWithTag("Crowd");
        StartCoroutine(startIA());
    }

    private IEnumerator startIA() { // runs in parallel with RL
        foreach(string location in popPlan) {
            GameObject closest = pickClosest(location);
            visited.Add(closest);

            // TO-DO send closest.transform.position to RL
            nav.SetDestination(closest.transform.position); 
            // Debug.Log("Next Location: " + closest.name);
            yield return new WaitUntil(() => canSee(closest));

            if (covidAware) StartCoroutine(pickByCrowd(location, closest));
            yield return new WaitUntil(() => hasReached());
            // Debug.Log("Destination reached");
        }
        yield return null;
    }

    private bool canSee(GameObject closest) { // determines if agent can "see" its destination
        RaycastHit hit;
        Vector3 eyes = new Vector3(transform.position.x, 0.05f, transform.position.z);
        Ray eyesight = new Ray(eyes, this.transform.forward);

        if(Physics.Raycast(eyesight, out hit, 7f)) {
            if(hit.collider.name == closest.name && hit.transform.position.x == nav.destination.x
                                    && hit.transform.position.z == nav.destination.z) {
                // Debug.Log("Can see: " + closest.name);
                return true;
            }
        }
        return false;
    }

    private bool hasReached() { // determines if agent has reach its destination
        if (!nav.pathPending) { // is a path in the process of being computed?
            if (nav.remainingDistance <= nav.stoppingDistance) // has the agent reached the target?
                return true;
        }
        return false;
    }

    private GameObject pickClosest(string tag) { // picks the closest unvisited location given a tag
        GameObject[] locations = GameObject.FindGameObjectsWithTag(tag);
        GameObject best = null;
        float min = float.PositiveInfinity;//Vector3.Distance(transform.position, locations[0].transform.position);

        foreach (GameObject location in locations) { // find an unvisited location as minimum
            if (!visited.Contains(location)) {
                best = location;
                break;
            }
        }
        foreach (GameObject location in locations) { // find the closest unvisited location
            float dist = Vector3.Distance(transform.position, location.transform.position);
            if(dist < min && !visited.Contains(location)) {
                min = dist;
                best = location;
            }
        }
        return best;
    }

    private IEnumerator pickByCrowd(string tag, GameObject closest) { // CovidAware decision making
        GameObject[] locations = GameObject.FindGameObjectsWithTag(tag);

        // pick the closest location, evaluate, and loop until out of locations or found 'good' location
        for (int i = 0; i < locations.Length; i++) { 
            int numPeople = numInProximity(closest);
            if (numPeople <= 1) {
                yield break;
            }

            closest = pickClosest(tag);
            visited.Add(closest);
            if (closest == null) continue;
            // TO-DO send closest.transform.position to RL
            nav.SetDestination(closest.transform.position); 
            // Debug.Log("Next location: " + closest.name);
            yield return new WaitUntil(() => canSee(closest));
        }
        yield break;
    }    

    private int numInProximity(GameObject sublocation) { // determines the number of people in proximity to a sublocation
        CapsuleCollider col = sublocation.GetComponent<CapsuleCollider>();
        col.radius = 3;
        int numPeople = 0;

        foreach(GameObject person in crowd) {
            if(col.bounds.Contains(person.transform.position)) 
                numPeople++;
        }
        // Debug.Log(sublocation.name + ": " + numPeople + " people");
        return numPeople;
    }
}
