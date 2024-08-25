using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointController : MonoBehaviour
{
    public WaypointManager waypointManager;
    public void SpawnSingleWaypoint()
    {
        waypointManager.SpawnSingleWaypoint();
    }
}
