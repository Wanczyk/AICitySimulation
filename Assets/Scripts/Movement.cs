using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Movement : MonoBehaviour
{
    public WheelCollider frontLeftWheelCollider;
    public WheelCollider frontRightWheelCollider;
    public float maxMotorTorque;
    public float maxBrakeTorque;
    public float maxSteeringAngle;
    public float maxSpeed = 100f;
    public float currentSpeed;
    public bool manual;
    
    private float _steering = 0;
    private float _throttle = 0;
    private float _brake = 0;

    public void SetSteering(float value)
    {
        _steering = Mathf.Clamp(value, -1, 1);
    }

    public void SetThrottle(float value)
    {
        _throttle = Mathf.Clamp(value, 0, 1);
    }
    
    public void SetBrake(float value)
    {
        _brake = Mathf.Clamp(value, 0, 1);
    }

    public float CurrentSpeed => currentSpeed;

    void Start()
    {
    }

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
        float steerAngle = maxSteeringAngle * _steering;
        
        frontLeftWheelCollider.steerAngle = steerAngle;
        frontRightWheelCollider.steerAngle = steerAngle;
    }

    void Drive()
    {
        currentSpeed = 2 * Mathf.PI * frontLeftWheelCollider.radius * frontLeftWheelCollider.rpm * 60 / 1000;
        
        float motorTorque = maxMotorTorque * _throttle;
        
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
        float brakeTorque = maxBrakeTorque * _brake;

        frontLeftWheelCollider.brakeTorque = brakeTorque;
        frontRightWheelCollider.brakeTorque = brakeTorque;
        
    }
}
