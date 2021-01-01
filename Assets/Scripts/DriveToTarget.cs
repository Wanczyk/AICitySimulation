using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Movement))]
public class DriveToTarget : Agent
{
    public float xSeconds = 3;
    public List<Transform> target;
    public Transform frontRaycast;
    public Transform rightRaycast;
    public Transform leftRaycast;
    public bool clockwise = true;
    private Rigidbody _rigidBody;
    private Vector3 _lastPosition;
    private Movement _movementController;
    private float _lastCheckTime;
    private bool _lastEpisodeGood;
    private int _nextCheckPointIndex = 1;
    private bool _targetReached = false;
    private float _oldDistance;
    private bool _firstEpisode = true;
    private float xBorder = 12;
    private float zBorder = 12;

    protected Vector3 AgentVelocity
    {
        get { return _rigidBody.velocity; }
    }
    protected Vector3 AgentPosition
    {
        get { return _rigidBody.position; }
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
        if(!clockwise)
        {
            _nextCheckPointIndex = target.Count;
        }
        _lastCheckTime = Time.time;
        _oldDistance = Vector3.Distance(
            this.transform.position,
            target[_nextCheckPointIndex - 1].transform.position
        );
        _rigidBody = GetComponent<Rigidbody>();
        _movementController = GetComponent<Movement>();
    }

    public void FixedUpdate()
    {
        if (!((Time.time - _lastCheckTime) > xSeconds) && _movementController.CurrentSpeed < 1) return;
        // CheckDistanceToTarget();
        RaycastObservations();
        _lastCheckTime = Time.time;
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
        if (!_targetReached && !_firstEpisode)
        {
            RespawnAgent();
        }
        _firstEpisode = false;
        _targetReached = false;
        _nextCheckPointIndex = 1;
        if(!clockwise)
        {
            _nextCheckPointIndex = target.Count;
        }
        RespawnTarget();
    }

    public override void OnActionReceived(float[] action)
    {
        _movementController.SetSteering(action[0]);
        // _movementController.SetBrake(action[1]);
        _movementController.SetThrottle(action[1]);
    } 

    void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.gameObject.CompareTag("Curb"))
        {
            AddReward(-1f);
            EndEpisode();
        }
        else if (collision.transform.gameObject.CompareTag("BackBumper"))
        {
            // AddReward(-0.5f);
            EndEpisode();
        }
        
    }
    
    private void OnTriggerEnter(Collider trigger)
    {
        // Debug.Log(trigger.transform.gameObject.name);
        if (trigger.transform.gameObject.name == "CheckPoint" + _nextCheckPointIndex.ToString())
        {
            AddReward(1f);
            if (_nextCheckPointIndex == target.Count && clockwise)
            {
                // _nextCheckPointIndex = 1;
                _targetReached = true;
                EndEpisode();
            }
            else if(_nextCheckPointIndex == 1 && !clockwise)
            {
                // _nextCheckPointIndex = 1;
                _targetReached = true;
                EndEpisode();
            }
            else
            {
                if(clockwise)
                {
                    _nextCheckPointIndex++;
                }
                else
                {
                    _nextCheckPointIndex--;
                }
            }
            // _targetReached = true;
            // EndEpisode();
        }
        else
        {
            EndEpisode();
        }
    }
    
    void OnCollisionExit (Collision collision)
    {
        // if(collision.transform.gameObject.CompareTag("Curb"))
        // {
        //     AddReward(0.1f);
        // }
    }

    public override void Heuristic(float[] actionsOut)
    {
        var action = new float[2];
        action[0] = Input.GetAxis("Horizontal");
        action[1] = Input.GetAxis("Vertical");
        // action[1] = Input.GetAxis("Jump");
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
        target[_nextCheckPointIndex - 1].localPosition = new Vector3(Random.Range(xBorder * -1, xBorder),
            0.5f,
            Random.Range(zBorder * -1, zBorder));
        target[_nextCheckPointIndex - 1].rotation = Quaternion.Euler(0, Random.Range(-180, 180), 0);
    }
    
    void CheckDistanceToTarget()
    {
        var distanceToTarget = Vector3.Distance(
            this.transform.position,
            target[_nextCheckPointIndex - 1].transform.position
        );

        if((int)(_oldDistance*1000) == (int)(distanceToTarget*1000))
        {
            AddReward(-0.1f);
            EndEpisode();
        }
        _oldDistance = distanceToTarget;
    }
    private void RaycastObservations()
    {
        RaycastHit objectHit;
        Vector3 frontRay = frontRaycast.transform.TransformDirection(Vector3.forward);
        if (Physics.Raycast(frontRaycast.transform.position, frontRay, out objectHit, 20))
        {
            if(objectHit.transform.tag=="Car" && objectHit.distance < 1f){
                AddReward(-2f);
                // Debug.Log("car close");
                EndEpisode();
            }
        }
        
        RaycastHit objectHitRight;
        RaycastHit objectHitLeft;
        Vector3 rightRay = rightRaycast.transform.TransformDirection(Vector3.forward);
        if (Physics.Raycast(rightRaycast.transform.position, rightRay, out objectHitRight, 4))
        {
            Vector3 leftRay = leftRaycast.transform.TransformDirection(Vector3.forward);
            if (Physics.Raycast(leftRaycast.transform.position, leftRay, out objectHitLeft, 4))
            {
                if(objectHitRight.transform.tag=="Curb" && objectHitLeft.transform.tag=="Curb"){
                    if ((objectHitLeft.distance > objectHitRight.distance))
                    {
                        // Debug.Log("OK");
                        AddReward(0.001f);
                    }
                    else
                    {
                        // Debug.Log("To close left");
                        // AddReward(-0.01f);
                        // EndEpisode();
                    }
                }
            }
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 dirToTarget = (target[_nextCheckPointIndex - 1].position - this.transform.position).normalized;
        sensor.AddObservation(
            this.transform.InverseTransformPoint(target[_nextCheckPointIndex - 1].transform.position));
        sensor.AddObservation(
            this.transform.InverseTransformVector(AgentVelocity));
        sensor.AddObservation(
            this.transform.InverseTransformDirection(dirToTarget));
    }
}
