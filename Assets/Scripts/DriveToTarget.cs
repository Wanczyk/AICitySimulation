using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Movement))]
public class DriveToTarget : Agent
{
    public Transform target;
    public Renderer ground;
    private Rigidbody m_rbody;
    private Movement m_controller;
    public int maxStep;
    private float m_rewardScalar = 0.0001f;
    private bool m_targetReached = false;
    
    private float old_distance = 9999999999f;

    protected Vector3 AgentVelocity
    {
        get { return m_rbody.velocity; }
    }
    

    protected float RewardScalar
    {
        get { return m_rewardScalar; }
    }

    void Start()
    {
        m_rbody = GetComponent<Rigidbody>();
        m_controller = GetComponent<Movement>();

        // If the agent's maxStep > 0, use it to calculate the
        // reward scalar. To be used for per-step rewards
        if (maxStep > 0)
        {
            m_rewardScalar = 1f / maxStep;
        }
    }

    public void FixedUpdate()
    {
        // CheckDistanceToTarget();

        // If the car falls off the platform, end episode
        if (this.transform.localPosition.y < 0)
        {
            EndEpisode();
        }
    }

    public override void OnEpisodeBegin()
    {
        // Only respawn the agent if we didn't reach the target
        if (!m_targetReached)
            RespawnAgent();
        m_targetReached = false;
        RespawnTarget();
    }

    public override void OnActionReceived(float[] action)
    {
        m_controller.SetSteering(action[0]);
        m_controller.SetThrottle(action[1]);
        m_controller.SetBrake(action[2]);
    }

    void OnCollisionEnter(Collision collision)
    {
        // if(collision.transform.gameObject.tag == "Target")
        // {
        //     Debug.Log("HitTargetOnCollisionEnter");
        //     m_targetReached = true;
        //     AddReward(1f);
        //     EndEpisode();
        // }
        
    }

    public override void Heuristic(float[] actionsOut)
    {
        var action = new float[3];
        action[0] = Input.GetAxis("Horizontal");
        action[1] = Input.GetAxis("Vertical");
        action[2] = Input.GetAxis("Jump");
    }

    void CheckDistanceToTarget()
    {
        var distanceToTarget = Vector3.Distance(
            this.transform.position,
            target.transform.position
        );
        // Upon reaching the target, respawn it to a random position
        // and add reward of +1
            Debug.Log(distanceToTarget);
        if (distanceToTarget < 1)
        {
            Debug.Log("Target!!");
            m_targetReached = true;
            AddReward(1f);
            EndEpisode();
        }
        old_distance = distanceToTarget;
    }

    void RespawnAgent()
    {
        Debug.Log("RespawnAgent");
        m_controller.SetThrottle(0);
        m_controller.SetSteering(0);
        m_controller.SetBrake(1);
        m_rbody.velocity = Vector3.zero;
        m_rbody.angularVelocity = Vector3.zero;
        this.transform.localPosition = new Vector3(0, 1f, 0);
        this.transform.rotation = Quaternion.Euler(0, Random.Range(-180, 180), 0);
    }

    void RespawnTarget()
    {
        Debug.Log("RespawnTarget");
        Vector3 extents = ground.bounds.extents - (Vector3.one * 3);
        target.localPosition = new Vector3(Random.Range(-extents.x, extents.x),
                                           0.5f,
                                           Random.Range(-extents.z, extents.z));
        target.rotation = Quaternion.Euler(0, Random.Range(-180, 180), 0);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 dirToTarget = (target.position - this.transform.position).normalized;

        // Target position in agent frame
        sensor.AddObservation(
            this.transform.InverseTransformPoint(target.transform.position)); // vec 3

        // Agent velocity in agent frame
        sensor.AddObservation(
            this.transform.InverseTransformVector(AgentVelocity)); // vec 3

        // Direction to target in agent frame
        sensor.AddObservation(
            this.transform.InverseTransformDirection(dirToTarget)); // vec 3
    }
}
