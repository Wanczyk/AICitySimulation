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
    
    private float m_steering = 0;
    private float m_throttle = 0;
    private float m_brake = 0;

    public void SetSteering(float value)
    {
        m_steering = Mathf.Clamp(value, -1, 1);
    }

    public void SetThrottle(float value)
    {
        m_throttle = Mathf.Clamp(value, -1, 1);
    }

    public void SetBrake(float value)
    {
        m_brake = Mathf.Clamp(value, 0, 1);
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float steerAngle = maxSteeringAngle * m_steering;
        float motorTorque = maxMotorTorque * m_throttle;
        float brakeTorque = maxBrakeTorque * m_brake;


        // float motor = maxMotorTorque * Input.GetAxis("Vertical");
        // float steering = maxSteeringAngle * Input.GetAxis("Horizontal");
        // float brake = maxBrakeTorque * Input.GetAxis("Jump");

        // backLeftWheelCollider.motorTorque = motorTorque;
        // backRightWheelCollider.motorTorque = motorTorque;

        frontLeftWheelCollider.steerAngle = steerAngle;
        frontRightWheelCollider.steerAngle = steerAngle;

        frontLeftWheelCollider.motorTorque = motorTorque;
        frontRightWheelCollider.motorTorque = motorTorque;

        frontLeftWheelCollider.brakeTorque = brakeTorque;
        frontRightWheelCollider.brakeTorque = brakeTorque;

        // frontLeftWheelCollider.steerAngle = steering;
        // frontRightWheelCollider.steerAngle = steering;
        
        // frontLeftWheelCollider.motorTorque = motor;
        // frontRightWheelCollider.motorTorque = motor;
        
        // frontLeftWheelCollider.brakeTorque = brake;
        // frontRightWheelCollider.brakeTorque = brake;
        
        // frontLeftWheel.transform.localRotation = Quaternion.Slerp(frontLeftWheel.transform.localRotation, Quaternion.Euler(0f, 90f + steerAngle, 0f), Time.deltaTime);
        // frontRightWheel.transform.localRotation = Quaternion.Slerp(frontRightWheel.transform.localRotation, Quaternion.Euler(180f, 90f + steerAngle, 0f), Time.deltaTime);

        
    }
}
