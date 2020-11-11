using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEditor;
using UnityEngine.UIElements;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Movement))]
public class DriveToTarget : Agent
{
    public float minDistance;
    public List<Transform> target;
    public Transform street;
    public Transform frontRaycast;
    public Transform backRaycast;
    public Transform leftRaycast;
    public Transform rightRaycast;
    public GameObject frontBumper;
    public GameObject backBumper;
    private Rigidbody _rigidBody;
    public Material material;
    private Movement _movementController;
    private bool _targetReached = false;
    
    private float _oldDistance = 9999999999f;

    private float _lastCheckTime;
    private const float XSeconds = 6.0f;

    private int _targetIndex = 0;

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
    protected string CarName
    {
        get { return this.name; }
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
            AddReward(0.0005f);
        }
        
        if (this.transform.localPosition.y < 0)
        {
            OnEndEpisode();
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
        _movementController.SetBrake(action[2]);
    } 

    void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.gameObject.CompareTag("Curb"))
        {
            // Debug.Log("Curb");
            AddReward(-0.8f);
            OnEndEpisode();
        }
    }
    
    void OnCollisionExit (Collision collision)
    {
        if(collision.transform.gameObject.CompareTag("Curb"))
        {
            // Debug.Log("Exit Curb");
            AddReward(0.5f);
        }
    }
    
    private void OnTriggerEnter(Collider colliderDetection)
    {
        if(colliderDetection.transform.gameObject.GetInstanceID() == target[_targetIndex].GetInstanceID())
        {
            
        }
    }

    public override void Heuristic(float[] actionsOut)
    {
        var action = new float[3];
        action[0] = Input.GetAxis("Horizontal");
        action[1] = Input.GetAxis("Vertical");
        action[2] = Input.GetAxis("Jump");
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
                // OnEndEpisode();
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

    private void OnEndEpisode()
    {
        Material destroyMaterial = target[_targetIndex].GetComponent<MeshRenderer>().material;
        Destroy(destroyMaterial);
        // target[_targetIndex].tag = "Untagged";
        EndEpisode();
    }

    void CheckDistanceToTarget()
    {
        var distanceToTarget = Vector3.Distance(
            this.transform.position,
            target[_targetIndex].transform.position
        );

        if (distanceToTarget < minDistance)
        {
            Debug.Log("Target Reached!!");
            _targetReached = true;
            AddReward(1f);
            OnEndEpisode();
        }
        
        if (distanceToTarget < _oldDistance)
        {
            // AddReward(1f * (1/distanceToTarget));
        }
        
        if ((Time.time - _lastCheckTime) > XSeconds*10)
        {
            if((int)(_oldDistance*10000) == (int)(distanceToTarget*10000))
            {
                // AddReward(-0.1f);
                OnEndEpisode();
            }
            _lastCheckTime = Time.time;
        }
        _oldDistance = distanceToTarget;
    }

    void RespawnAgent()
    {
        _movementController.SetThrottle(0);
        _movementController.SetSteering(0);
        _movementController.SetBrake(1);
        _rigidBody.velocity = Vector3.zero;
        _rigidBody.angularVelocity = Vector3.zero;
        
        //street
        int x = 0;
        int z = 0;
        // int randomNumber1 = Random.Range(0, 1);
        // int x = 0;
        // int z = 0;
        // if (randomNumber1 == 0)
        // {
        //     int randomNumber2 = Random.Range(0, 2);
        //     if (randomNumber2 == 0)
        //     {
        //         //siple drive to target
        //         z = Random.Range(-20, 20);
        //         x = -20;
        //         
        //         
        //         //CompleteCity
        //         // x = -79;
        //         // z = Random.Range(-62, 62);
        //     }
        //     else if (randomNumber2 == 1)
        //     {
        //         //siple drive to target
        //         z = Random.Range(-20, 20);
        //         x = 0; 
        //         
        //         //CompleteCity
        //         // x = 0;
        //         // z = Random.Range(-62, 62);
        //     }
        //     else if (randomNumber2 == 2)
        //     {
        //         //siple drive to target
        //         z = Random.Range(-20, 20);
        //         x = 20;
        //         
        //         //CompleteCity
        //         // x = 79;
        //         // z = Random.Range(-62, 62);
        //     }
        // }
        // else
        // {
        //     int randomNumber2 = Random.Range(0, 2);
        //     if (randomNumber2 == 0)
        //     {
        //         //siple drive to target
        //         x = Random.Range(-20, 20);
        //         z = -20;
        //         
        //         //CompleteCity
        //         //x = Random.Range(-70, 70);
        //         // z = -72;
        //     }
        //     else if (randomNumber2 == 1)
        //     {
        //         //siple drive to target
        //         x = Random.Range(-20, 20);
        //         
        //         //CompleteCity
        //         // x = Random.Range(-70, 70);
        //         z = 0;
        //     }
        //     else if (randomNumber2 == 2)
        //     {
        //         //siple drive to target
        //         x = Random.Range(-20, 20);
        //         z = 20;
        //         
        //         //CompleteCity
        //         // x = Random.Range(-70, 70);
        //         // z = 72;
        //     }
        // }
        this.transform.localPosition = new Vector3(x, 1f, z);
        this.transform.rotation = Quaternion.Euler(0, Random.Range(-180, 180), 0);
    }

    void RespawnTarget()
    {
        _targetIndex = Random.Range(0, target.Count);
        // do
        // {
        //     _targetIndex = Random.Range(0, target.Count);
        //     Debug.Log(_targetIndex);
        // } while (!target[_targetIndex].CompareTag("Untagged"));
        //
        target[_targetIndex].GetComponent<MeshRenderer>().material = material;
        // target[_targetIndex].tag = CarName;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 dirToTarget = (target[_targetIndex].position - this.transform.position).normalized;

        sensor.AddObservation(
            this.transform.InverseTransformPoint(target[_targetIndex].transform.position)); // vec 3

        sensor.AddObservation(
            this.transform.InverseTransformVector(AgentVelocity)); // vec 3

        sensor.AddObservation(
            this.transform.InverseTransformDirection(dirToTarget)); // vec 3
    }
}
