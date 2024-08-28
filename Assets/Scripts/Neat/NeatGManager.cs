using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeatGManager : MonoBehaviour
{
    public GameObject NeatJetPrefab;

    public GameObject[] allNeatJets;
    public NeatNetwork[] allNeatNetworks;

    // Waypoint management
    public Transform[] waypoints; // Array to hold waypoints in order
    public int[] currentWaypointIndex; // Track each jet's current waypoint

    public int inputNodes, outputNodes, hiddenNodes;

    [SerializeField] public int currentGeneration = 0;
    public int startingPopulation = 20;
    
    // Speciation
    public List<NeatSpecies> allSpecies;
    public Dictionary<NeatGenome, NeatSpecies> allSpeciesDic;
    public float compThresh = 0.5f;

    public int keepBest, leaveWorst;

    public int currentAlive;
    private bool repoping = false;

    public bool spawnFromSave = false;
    public bool saveOnEnd = false;
    public int bestTime = 100;
    public int addToBest = 50;

    public float c1;
    public float c2;
    public float c3;

    void Start()
    {
        allNeatJets = new GameObject[startingPopulation];
        allNeatNetworks = new NeatNetwork[startingPopulation];
        currentWaypointIndex = new int[startingPopulation];
        allSpecies = new List<NeatSpecies>();
    
        // Fetch the WaypointManager and its waypoints
        WaypointManager waypointManager = GameObject.FindObjectOfType<WaypointManager>();
        if (waypointManager != null)
        {
            GameObject[] waypointObjects = waypointManager.GetWaypoints();
            waypoints = new Transform[waypointObjects.Length];
            for (int i = 0; i < waypointObjects.Length; i++)
            {
                waypoints[i] = waypointObjects[i].transform;
            }
        }
        else
        {
            Debug.LogError("WaypointManager not found. Ensure it is present in the scene.");
        }
    
        if (spawnFromSave)
        {
            StartingSavedNetworks();
        }
        else 
        {
            StartingNetworks();
        }
    
        MutatePopulation();
        SpeciatePopulation();
        
        SpawnBody();
        currentGeneration += 1;
    }


    void FixedUpdate()
    {
        currentAlive = CurrentAlive();
        if (repoping == false && currentAlive <= 0)
        {
            repoping = true;
            Repopulate();
            repoping = false;
        }
    }

    public int CurrentAlive()
    {
        int alive = 0;
        for (int i = 0; i < allNeatJets.Length; i++)
        {
            if (allNeatJets[i].gameObject)
            {
                alive++;
            }
        }

        return alive;
    }

    private void Repopulate()
    {
        if (spawnFromSave)
        {
            bestTime += addToBest;
            StartingSavedNetworks();
        }

        else
        {
            SortPopulation();
            SetNewPopulationNetworks();
        }

        MutatePopulation();
        SpeciatePopulation();

        // GameObject.FindObjectOfType<WaypointManager>().DestroyWaypoint(); //TODO: Will need to get rid of
        // GameObject.FindObjectOfType<WaypointManager>().SpawnWaypoint();
        SpawnBody();
        currentGeneration += 1;
    }

    private void MutatePopulation()
    {
        for (int i = keepBest; i < startingPopulation; i++)
        {
            allNeatNetworks[i].MutateNetwork();
        }
    }

    private void SpeciatePopulation()
    {
        allSpeciesDic = new Dictionary<NeatGenome, NeatSpecies>();
        for (int i = 0; i < startingPopulation; i++)
        {
            NeatGenome genome = allNeatNetworks[i].myGenome;
            bool found = false;

            foreach (NeatSpecies spec in allSpecies)
            {
                float compDis = NeatUtilities.GetCompatabilityDistance(genome, spec.mascot, c1, c2, c3);

                if (compDis < compThresh)
                {
                    spec.members.Add(genome);
                    allSpeciesDic.Add(genome, spec);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                NeatSpecies spec = new NeatSpecies(genome);
                allSpecies.Add(spec);
                allSpeciesDic.Add(genome, spec);
            }
        }

        for (int i = allSpecies.Count - 1 ; i >= 0; i--)
        {
            if (allSpecies[i].members.Count == 0)
            {
                // If any species members are empty, remove it
                allSpecies.RemoveAt(i);
            }

            else
            {
                // Randomize the mascot
                allSpecies[i].mascot = allSpecies[i].members[Random.Range(0, allSpecies[i].members.Count)];
            }
        }
    }

    private void SortPopulation()
    {
        for (int i = 0; i < allNeatNetworks.Length; i++)
        {
            for (int j = i; j < allNeatNetworks.Length; j++)
            {
                if (allNeatNetworks[i].fitness < allNeatNetworks[j].fitness)
                {
                    NeatNetwork temp = allNeatNetworks[i];
                    allNeatNetworks[i] = allNeatNetworks[j];
                    allNeatNetworks[j] = temp;
                }
            }
        }
    }

    public void BestFound()
    {
        spawnFromSave = true;
    }

    private void SetNewPopulationNetworks()
    {
        NeatNetwork[] newPopulation = new NeatNetwork[startingPopulation];
        for (int i = 0; i < startingPopulation - leaveWorst; i++)
        {
            newPopulation[i] = allNeatNetworks[i];
        }

        for (int i = startingPopulation - leaveWorst; i < startingPopulation; i++)
        {
            newPopulation[i] = new NeatNetwork(inputNodes, outputNodes, hiddenNodes);
        }

        allNeatNetworks = newPopulation;
    }

    // Create initial network
    private void StartingNetworks()
    {
        /*
            Creates initial group of networks from StartingPopulation integer.
        */
        spawnFromSave = false;
        for (int i = 0; i < startingPopulation; i++)
        {
            allNeatNetworks[i] = new NeatNetwork(inputNodes, outputNodes, hiddenNodes);
        }
    }

    private void StartingSavedNetworks()
    {
       /*
            Creates initial group of networks from saved network.
       */
       for (int i = 0; i < startingPopulation; i++)
       {
        allNeatNetworks[i] = new NeatNetwork(NeatUtilities.LoadGenome());
       }
    }

    private void SpawnBody()
    {
        /* Creates initial group of jet GameObjects from StartingPopulation integer.
           and matches jetObjects to their Network Brains. */

        for (int i = 0; i < startingPopulation; i++)
        {
            Vector3 pos = new Vector3(0, 10, 0);
            allNeatJets[i] = Instantiate(NeatJetPrefab, pos, transform.rotation);
            JetController jetController = allNeatJets[i].GetComponent<JetController>();

            // Assign the network and waypoint details
            jetController.myBrainIndex = i;
            jetController.myNetwork = allNeatNetworks[i];
            jetController.inputNodes = inputNodes;
            jetController.outputNodes = outputNodes;
            jetController.hiddenNodes = hiddenNodes;
            jetController.myColor = allSpeciesDic[allNeatNetworks[i].myGenome].speciesColor;
            jetController.currentWaypoint = waypoints[0];  // Start with the first waypoint

            currentWaypointIndex[i] = 0;
        }
    }

    public void UpdateJetWaypoint(int jetIndex)
    {
        currentWaypointIndex[jetIndex]++;
        if (currentWaypointIndex[jetIndex] < waypoints.Length)
        {
            allNeatJets[jetIndex].GetComponent<JetController>().currentWaypoint = waypoints[currentWaypointIndex[jetIndex]];
        }
        else
        {
            // Handle case where all waypoints are completed
            allNeatJets[jetIndex].GetComponent<JetController>().currentWaypoint = null;
        }
    }

    public void Death(float fitness, int index)
    {
        allNeatNetworks[index].fitness = fitness;
    }
}
