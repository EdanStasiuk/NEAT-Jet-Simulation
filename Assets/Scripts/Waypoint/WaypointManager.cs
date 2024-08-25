using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WaypointManager : MonoBehaviour
{
    public GameObject waypointPrefab;
    public GameObject textPrefab;
    public int waypointCount;
    public int curAlive;
    private GameObject[] waypoints;
    private bool repoping = false;

    // Start is called before the first frame update
    void Awake()
    {
        curAlive = waypointCount;
        waypoints = new GameObject[waypointCount];
        SpawnWaypointTrack();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        curAlive = CurrentAlive();
        if (repoping == false && curAlive <= 0)
        {
            repoping = true;
            SpawnWaypointTrack();
            repoping = false;
        }
    }

    public int CurrentAlive()
    {
        WaypointController[] localWaypoints = FindObjectsOfType<WaypointController>();
        return localWaypoints.Length;
    }

    public void DestroyWaypoint()
    {
        WaypointController[] localWaypoints = FindObjectsOfType<WaypointController>();
        foreach (WaypointController waypoint in localWaypoints)
        {
            Destroy(waypoint.gameObject);
        }
    }

    public void SpawnWaypoint()
    {
        for (int i = 0; i < waypointCount; i++)
        {
            Vector3 pos = new Vector3(Random.value, Random.value, 30);
            pos = Camera.main.ViewportToWorldPoint(pos);

            waypoints[i] = Instantiate(waypointPrefab, pos, transform.rotation);
            waypoints[i].gameObject.GetComponent<WaypointController>().waypointManager = this;
        }
    }

    public void SpawnSingleWaypoint()
    {
        Vector3 pos = new Vector3(Random.value, Random.value, 30);
        pos = Camera.main.ViewportToWorldPoint(pos);

        GameObject localWaypoint = Instantiate(waypointPrefab, pos, transform.rotation);
        localWaypoint.gameObject.GetComponent<WaypointController>().waypointManager = this;
    }

    public void SpawnWaypointTrack()
    {
        // Ensure waypointCount is set correctly
        if (waypointCount < 8)
        {
            waypointCount = 8;
            waypoints = new GameObject[waypointCount];
        }

        // Hardcoded positions
        Vector3[] positions = new Vector3[]
        {
            new Vector3(0, 10, 10),
            new Vector3(7, 10, 25),
            new Vector3(20, 15, 45),
            new Vector3(35, 6, 60),
            new Vector3(15, 10, 70),
            new Vector3(-5, 20, 85),
            new Vector3(-15, 30, 100),
            new Vector3(-30, 10, 125),
        };

        char[] labels = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H' };

        // Spawn waypoints at each position
        for (int i = 0; i < positions.Length; i++)
        {
            waypoints[i] = Instantiate(waypointPrefab, positions[i], Quaternion.identity);
            waypoints[i].GetComponent<WaypointController>().waypointManager = this;

            // Create a label above each waypoint
            Vector3 labelPosition = positions[i] + new Vector3(0, 1, 0); // Adjust Y offset as needed
            GameObject label = Instantiate(textPrefab, labelPosition, Quaternion.identity);
            TextMeshPro textMesh = label.GetComponent<TextMeshPro>();

            textMesh.text = labels[i].ToString();
            textMesh.fontSize = 5; // Adjust size as needed
            textMesh.alignment = TextAlignmentOptions.Center;
            
            label.transform.SetParent(waypoints[i].transform); // Parent label to the waypoint
        }
    }

    public GameObject[] GetWaypoints()
    {
        return waypoints;
    }
}
