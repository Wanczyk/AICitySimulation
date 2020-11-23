using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Movement : MonoBehaviour
{
    public WheelCollider frontLeftWheelCollider;
    public WheelCollider frontRightWheelCollider;
    public WheelCollider backLeftWheelCollider;
    public WheelCollider backRightWheelCollider;
    public GameObject frontLeftWheel;
    public GameObject frontRightWheel;
    public float maxMotorTorque;
    public float maxBrakeTorque;
    public float maxSteeringAngle;
    public float maxSpeed = 100f;
    public float currentSpeed;
    public bool manual;
    
    private float m_steering = 0;
    private float m_throttle = 0;
    private float m_brake = 0;

    public void SetSteering(float value)
    {
        m_steering = Mathf.Clamp(value, -1, 1);
    }

    public void SetThrottle(float value)
    {
        m_throttle = Mathf.Clamp(value, 0, 1);
    }
    
    public void SetBrake(float value)
    {
        m_brake = Mathf.Clamp(value, 0, 1);
    }

    public float CurrentSpeed => currentSpeed;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (manual)
        {
            DriveManual();
        }
        else
        {
            DriveAI();
        }
    }

    void DriveAI()
    {
        Steer();
        Drive();
        Brake();
    }
    void DriveManual()
    {
        float motor = maxMotorTorque * Input.GetAxis("Vertical");
        float steering = maxSteeringAngle * Input.GetAxis("Horizontal");
        float brake = maxBrakeTorque * Input.GetAxis("Jump");

        currentSpeed = 2 * Mathf.PI * frontLeftWheelCollider.radius * frontLeftWheelCollider.rpm * 60 / 1000;
        
        frontLeftWheelCollider.steerAngle = steering;
        frontRightWheelCollider.steerAngle = steering;

        frontLeftWheelCollider.brakeTorque = brake;
        frontRightWheelCollider.brakeTorque = brake;
        
        if (currentSpeed < maxSpeed) {
            frontLeftWheelCollider.motorTorque = motor;
            frontRightWheelCollider.motorTorque = motor;
        } else {
            frontLeftWheelCollider.motorTorque = 0;
            frontRightWheelCollider.motorTorque = 0;
        }
    }

    void Steer()
    {
        float steerAngle = maxSteeringAngle * m_steering;
        
        frontLeftWheelCollider.steerAngle = steerAngle;
        frontRightWheelCollider.steerAngle = steerAngle;
    }

    void Drive()
    {
        currentSpeed = 2 * Mathf.PI * frontLeftWheelCollider.radius * frontLeftWheelCollider.rpm * 60 / 1000;
        
        float motorTorque = maxMotorTorque * m_throttle;
        
        if (currentSpeed < maxSpeed) {
            frontLeftWheelCollider.motorTorque = motorTorque;
            frontRightWheelCollider.motorTorque = motorTorque;
        } else {
            frontLeftWheelCollider.motorTorque = 0;
            frontRightWheelCollider.motorTorque = 0;
        }
    }

    void Brake()
    {
        float brakeTorque = maxBrakeTorque * m_brake;

        frontLeftWheelCollider.brakeTorque = brakeTorque;
        frontRightWheelCollider.brakeTorque = brakeTorque;
        
    }
}
