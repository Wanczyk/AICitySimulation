using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Movement))]
public class a : Agent
{
    public int xBorder;
    public int zBorder;
    public float minDistance;
    public Transform target;
    public Transform frontRaycast;
    public Transform leftRaycast;
    public Transform rightRaycast;
    public float xSeconds = 3;
    private Rigidbody _rigidBody;
    private Movement _movementController;
    private bool _targetReached = false;
    private float _oldDistance = 9999999f;
    private float _lastCheckTime;

    protected Vector3 AgentVelocity
    {
        get { return _rigidBody.velocity; }
    }
    protected Vector3 AgentLocalVelocity
    {
        get
        {
            Vector3 localVelocity = transform.InverseTransformDirection(_rigidBody.velocity);
            return localVelocity;
        }
    }

    void Start()
    {
        _lastCheckTime = Time.time;
        _rigidBody = GetComponent<Rigidbody>();
        _movementController = GetComponent<Movement>();
    }

    public void FixedUpdate()
    {
        CheckDistanceToTarget();
        // RaycastObservations();
        
        if (AgentLocalVelocity.z > 0.01f)
        {
            AddReward(0.001f);
        }
        
        if (this.transform.localPosition.y < 0 || 
            this.transform.localPosition.x > xBorder || 
            this.transform.localPosition.y > zBorder || 
            this.transform.localPosition.x < xBorder * -1 || 
            this.transform.localPosition.y < zBorder * -1)
        {
            EndEpisode();
        }
    }

    public override void OnEpisodeBegin()
    {
        if (!_targetReached)
        {
            RespawnAgent();
        }
        _targetReached = false;
        RespawnTarget();
    }

    public override void OnActionReceived(float[] action)
    {
        _movementController.SetSteering(action[0]);
        _movementController.SetThrottle(action[1]);
        // _movementController.SetBrake(action[2]);
    } 

    void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.gameObject.CompareTag("Curb"))
        {
            EndEpisode();
        }
    }
    
    void OnCollisionExit (Collision collision)
    {
        if(collision.transform.gameObject.CompareTag("Curb"))
        {
        }
    }

    public override void Heuristic(float[] actionsOut)
    {
        var action = new float[2];
        action[0] = Input.GetAxis("Horizontal");
        action[1] = Input.GetAxis("Vertical");
        // action[2] = Input.GetAxis("Jump");
    }

    private void RaycastObservations()
    {
        RaycastHit objectHit;
        Vector3 frontRay = frontRaycast.transform.TransformDirection(Vector3.forward);
        Debug.DrawLine(frontRaycast.transform.position, frontRaycast.transform.TransformDirection(Vector3.forward), Color.green);
        if (Physics.Raycast(frontRaycast.transform.position, frontRay, out objectHit, 20))
        {
            if(objectHit.transform.tag=="Car" && objectHit.distance < 2f){
                // AddReward(-0.5f);
                // EndEpisode();
                Debug.Log("car close");
            }
        }
        
        RaycastHit objectHitRight;
        RaycastHit objectHitLeft;
        Vector3 rightRay = rightRaycast.transform.TransformDirection(Vector3.forward);
        if (Physics.Raycast(rightRaycast.transform.position, rightRay, out objectHitRight, 20))
        {
            Vector3 leftRay = leftRaycast.transform.TransformDirection(Vector3.forward);
            if (Physics.Raycast(leftRaycast.transform.position, leftRay, out objectHitLeft, 20))
            {
                if(objectHitRight.transform.tag=="Curb" && objectHitLeft.transform.tag=="Curb"){
                    if ((objectHitLeft.distance > objectHitRight.distance) && AgentLocalVelocity.z > 0.1f)
                    {
                        if (objectHitRight.distance < 0.5f)
                        {
                            // AddReward(-0.08f);
                        }
                        else
                        {
                            // AddReward(0.05f);
                        }
                    }
                    else
                    {
                        // AddReward(-0.1f);
                    }
                }
            }
        }
    }

    void CheckDistanceToTarget()
    {
        var distanceToTarget = Vector3.Distance(
            this.transform.position,
            target.transform.position
        );

        if (distanceToTarget < minDistance)
        {
            Debug.Log("Target Reached!!");
            _targetReached = true;
            AddReward(1f);
            EndEpisode();
        }
        
        if (distanceToTarget < _oldDistance)
        {
            // AddReward(0.1f * (1/distanceToTarget));
        }
        
        if ((Time.time - _lastCheckTime) > xSeconds)
        {
            if((int)(_oldDistance*10000) == (int)(distanceToTarget*10000))
            {
                // AddReward(-0.1f);
                EndEpisode();
            }
            _lastCheckTime = Time.time;
            _oldDistance = distanceToTarget;
        }
        _oldDistance = distanceToTarget;
    }

    void RespawnAgent()
    {
        _movementController.SetThrottle(1);
        _movementController.SetSteering(0);
        // _movementController.SetBrake(1);
        _rigidBody.velocity = Vector3.zero;
        _rigidBody.angularVelocity = Vector3.zero;

        this.transform.localPosition = new Vector3(Random.Range(xBorder * -1, xBorder), 1f, Random.Range(zBorder * -1, zBorder));
        this.transform.rotation = Quaternion.Euler(0, Random.Range(-180, 180), 0);
    }

    void RespawnTarget()
    {
        target.localPosition = new Vector3(Random.Range(xBorder * -1, xBorder),
            0.5f,
            Random.Range(zBorder * -1, zBorder));
        target.rotation = Quaternion.Euler(0, Random.Range(-180, 180), 0);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 dirToTarget = (target.position - this.transform.position).normalized;

        sensor.AddObservation(
            this.transform.InverseTransformPoint(target.transform.position)); // vec 3

        sensor.AddObservation(
            this.transform.InverseTransformVector(AgentVelocity)); // vec 3

        sensor.AddObservation(
            this.transform.InverseTransformDirection(dirToTarget)); // vec 3
    }
}
