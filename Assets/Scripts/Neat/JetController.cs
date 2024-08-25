using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetController : MonoBehaviour
{
    public NeatNetwork myNetwork;

    private float[] sensors;

    private float hitDivider = 10f;
    private float rayDistance = 80f;

    [Header("Energy Options")]
    public float totalEnergy; // Starting energy level
    public float rewardEnergy;
    public float currentEnergy;

    [Header("Fitness Options")]
    public float overallFitness = 0;
    public float foodMultiplier;
    public float waypointsSinceStart = 0f;

    [Header("Network Settings")]

    public int myBrainIndex;

    [SerializeField] private float surviveTime = 0;
    [SerializeField] private int bestTime = 0;

    public Color myColor;

    // [Range(-1f,1f)]
    // public float acceleration, turning;

    public int inputNodes, outputNodes, hiddenNodes;

    public Transform currentWaypoint; // Next waypoint to hit
    private float lastThrust = 0f;
    private float alignmentThreshold = 0.95f; // Cosine of the angle threshold (e.g., 0.95 means within ~18 degrees)
    private float minimumThrustThreshold = 0.5f;

    void Start()
    {
        // gameObject.GetComponent<Renderer>().material.color = myColor;
        // gameObject.GetComponent<TrailRenderer>().startColor = myColor;

        currentEnergy = totalEnergy;
        sensors = new float[inputNodes];
    }

    void Awake()
    {
        bestTime = GameObject.FindObjectOfType<NeatGManager>().bestTime;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        InputSensors();
        float[] outputs = myNetwork.FeedForwardNetwork(sensors);

        // Assume outputs[0] is thrust, outputs[1] is pitch (vertical), and outputs[2] is yaw (horizontal), and outputs[3] is roll
        MoveJet(outputs[0], outputs[1], outputs[2], outputs[3]);

        lastThrust = outputs[0];
        
        CalculateFitness();
        surviveTime += Time.deltaTime;
    }

    private void CalculateFitness()
    {
        UpdateEnergy();

        if (currentWaypoint != null)
        {
            float distanceToWaypoint = Vector3.Distance(transform.position, currentWaypoint.position);
            Vector3 directionToWaypoint = (currentWaypoint.position - transform.position).normalized;

            // Reward based on the angle between the forward vector and direction to the waypoint
            float alignment = Vector3.Dot(transform.forward, directionToWaypoint);

            // Only add alignment reward if there is forward thrust
            if (lastThrust >= minimumThrustThreshold && alignment >= alignmentThreshold)
            {
                currentEnergy += alignment*lastThrust;
            }

            if (distanceToWaypoint < 1f)  // Threshold for hitting the waypoint
            {
                overallFitness += 100f;
                GameObject.FindObjectOfType<NeatGManager>().UpdateJetWaypoint(myBrainIndex);
                currentEnergy += rewardEnergy;
                waypointsSinceStart += 1;
            }
        }
        else
        {
            // Last waypoint was reached
            overallFitness += 100f;
            Death();
        }

        if (currentEnergy <= 0 || surviveTime >= bestTime)
        {
            Death();
        }
    }


    private void UpdateEnergy()
    {
        currentEnergy -= Time.deltaTime;
    }

    private void Death()
    {
        GameObject.FindObjectOfType<NeatGManager>().Death(overallFitness, myBrainIndex); // No changes to overallFitness should be made after this line
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Wall")
        {
            // Debug.Log("Jet crashed");
            overallFitness = 0;
            Death();
        }
    }

    private void InputSensors()
    {
        Ray r = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        // Forward front
        if (Physics.Raycast(r, out hit, rayDistance))
        {
            if (hit.transform.tag == "Wall")
            {
                sensors[0] = hit.distance / hitDivider;
                Debug.DrawLine(r.origin, hit.point, Color.red);
            }
        }

        // // Forward right
        // r.direction = (transform.forward + transform.right);
        // if (Physics.Raycast(r, out hit, rayDistance))
        // {
        //     if (hit.transform.tag == "Wall")
        //     {
        //         sensors[1] = hit.distance / hitDivider;
        //         Debug.DrawLine(r.origin, hit.point, Color.red);
        //     }
        // }
        
        // // Forward left
        // r.direction = (transform.forward - transform.right);
        // if (Physics.Raycast(r, out hit, rayDistance))
        // {
        //     if (hit.transform.tag == "Wall")
        //     {
        //         sensors[2] = hit.distance / hitDivider;
        //         Debug.DrawLine(r.origin, hit.point, Color.red);
        //     }
        // }

        // Down
        r.direction = -transform.up;
        if (Physics.Raycast(r, out hit, rayDistance))
        {
            if (hit.transform.tag == "Wall")
            {
                sensors[1] = hit.distance / hitDivider;
                Debug.DrawLine(r.origin, hit.point, Color.red);
            }
        }

        if (currentWaypoint != null)
        {
            // Calculate direction to waypoint
            Vector3 directionToWaypoint = (currentWaypoint.position - transform.position).normalized;

            // Calculate relative direction in local space
            Vector3 localDirectionToWaypoint = transform.InverseTransformDirection(directionToWaypoint);

            // Use this information as input to the network
            sensors[2] = localDirectionToWaypoint.x; // Horizontal direction (-1 to 1)
            sensors[3] = localDirectionToWaypoint.y; // Vertical direction (-1 to 1)
            sensors[4] = localDirectionToWaypoint.z; // Forward direction (distance)

            // Optional: Calculate the angle between the forward vector and the direction to the waypoint
            float angleToWaypoint = Vector3.Angle(transform.forward, directionToWaypoint);
            sensors[5] = angleToWaypoint / 180f; // Normalized angle (0 to 1)
            Debug.DrawRay(transform.position, directionToWaypoint * rayDistance, Color.green);
        }
        else
        {
            sensors[2] = sensors[3] = sensors[4] = sensors[5] = 0; // No waypoint detected
        }
    }

    public void MoveJet(float thrust, float pitch, float yaw, float roll)
    {
        // Move forward continuously
        transform.position += transform.forward * System.Math.Abs(thrust * 8) * 2f * Time.deltaTime;
    
        // Handle vertical rotation (pitch), horizontal rotation (yaw), and roll rotation
        transform.Rotate(pitch * 90f * Time.deltaTime, yaw * 90f * Time.deltaTime, roll * 90f * Time.deltaTime, Space.Self);
    }

}
