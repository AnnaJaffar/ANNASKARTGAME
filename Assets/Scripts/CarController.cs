/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class CarController : MonoBehaviour
{
    public Transform steeringWheel;
    public Transform pivot;
    public Transform ExitPosition;
    public Collider carEnterCollider;
    public float accelerationSpeed = 10f;
    public float brakeSpeed = 10f;
    public float maxSteeringAngle = 135f;

    private InputActionMap actionMap;
    private InputAction accelerateAction;
    private InputAction brakeAction;
    private InputAction steerAction;
    private InputAction exitAction;
    private InputAction leftThumbstickAction;
    public Transform VrSteertransform;
    private float steerInput;
    public float maxSteerAngle = 30.0f;

    private bool isSeated = false;
    private float steeringAngle = 0f;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        var inputActionAsset = GetComponent<PlayerInput>().actions;
        actionMap = inputActionAsset.FindActionMap("Gameplay");

        accelerateAction = actionMap.FindAction("Accelerate");
        brakeAction = actionMap.FindAction("Brake");
        steerAction = actionMap.FindAction("Steer");
        exitAction = actionMap.FindAction("Exit");
        leftThumbstickAction = actionMap.FindAction("LeftThumbstick");

        accelerateAction.performed += ctx => Accelerate(ctx);
        brakeAction.performed += ctx => Brake(ctx);
        steerAction.performed += ctx => Steer(ctx);
        exitAction.performed += ctx => ExitCar();
        leftThumbstickAction.performed += ctx => LeftThumbstickControl(ctx);
    }

    private void OnEnable()
    {
        actionMap.Enable();
    }

    private void OnDisable()
    {
        actionMap.Disable();
    }

    public void SetSteerInputFromWheelRotation(float steeringWheelRotation)
    {
        // Convert the steering wheel rotation to a value between -1 and 1, assuming 360 degrees is the full range
        steerInput = (steeringWheelRotation / 360.0f) * maxSteerAngle;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == carEnterCollider)
        {
            isSeated = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == carEnterCollider)
        {
            isSeated = false;
        }
    }

    private void Accelerate(InputAction.CallbackContext context)
    {
        if (isSeated)
        {
            float triggerValue = context.ReadValue<float>();
            rb.AddForce(accelerationSpeed * triggerValue * transform.forward, ForceMode.Acceleration);
        }
    }

    private void Brake(InputAction.CallbackContext context)
    {
        if (isSeated)
        {
            float triggerValue = context.ReadValue<float>();
            rb.AddForce(brakeSpeed * triggerValue * -transform.forward, ForceMode.Acceleration);
        }
    }

    private void Steer(InputAction.CallbackContext context)
    {
        if (isSeated)
        {
            // Assuming VrSteertransform is your steering wheel object
            float maxSteerInput = 135f;
            float minSteerInput = -135f;
            float steerInput = -VrSteertransform.rotation.z * Mathf.Rad2Deg; // Convert radians to degrees

            steeringAngle = Mathf.Clamp(steerInput, minSteerInput, maxSteerInput);

            pivot.localRotation = Quaternion.Euler(0, steeringAngle, 0);
        }
    }


    private void ExitCar()
    {
        if (isSeated)
        {
            transform.position = ExitPosition.position;
            isSeated = false;
        }
    }

    private void LeftThumbstickControl(InputAction.CallbackContext context)
    {
        if (!isSeated)
        {
            Vector2 input = context.ReadValue<Vector2>();
            float steer = input.x * maxSteeringAngle;
            transform.Rotate(0, steer * Time.deltaTime, 0);

            if (input.y > 0)
            {
                rb.AddForce(accelerationSpeed * input.y * transform.forward, ForceMode.Acceleration);
            }
            else if (input.y < 0)
            {
                rb.AddForce(brakeSpeed * input.y * transform.forward, ForceMode.Acceleration);
            }
        }
    }
}*/


using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine.XR.Interaction.Toolkit;

public class CarController : MonoBehaviour
{
    public enum ControlMode
    {
        VrSteering,
        EverythingElse
    };

    public enum Axel
    {
        Front,
        Rear
    }

    [Serializable]
    public struct Wheel
    {
        public GameObject wheelModel;
        public WheelCollider wheelCollider;
        public GameObject wheelEffectObj;
        public ParticleSystem smokeParticle;
        public Axel axel;
    }

    public ControlMode control;

    public float maxAcceleration = 30.0f;
    public float brakeAcceleration = 50.0f;

    public float turnSensitivity = 1.0f;
    public float maxSteerAngle = 30.0f;

    public Vector3 _centerOfMass;

    public List<Wheel> wheels;

    float moveInput;
    float steerInput;
    float brakeInput;

    //float AccelerateInput;
    //float BrakeReverseInput;

    public float minSteerInput;
    public float maxSteerInput;

    private Rigidbody carRb;
    PlayerControl controls;
    public bool InputActions;
    public Transform VrSteertransform;
    private int Gear = 1;

    void Start()
    {
        carRb = GetComponent<Rigidbody>();
        carRb.centerOfMass = _centerOfMass;
    }

    void Awake()
    {
        if (InputActions == true)
        {
            controls = new PlayerControl();
            Debug.Log("Drive controls are set.");

            controls.Other.GearUp.started += ctx => Gear += 1;
            controls.Other.GearDown.started += ctx => Gear -= 1;
            controls.Other.CarMove.performed += ctx => moveInput = ctx.ReadValue<float>();
            controls.Other.CarMove.canceled += ctx => moveInput = 0f;
            controls.Other.Steer.performed += ctx => steerInput = ctx.ReadValue<float>();
            controls.Other.Steer.canceled += ctx => steerInput = 0f;
            controls.Other.Brake.performed += ctx => brakeInput = ctx.ReadValue<float>();
            controls.Other.Brake.canceled += ctx => brakeInput = 0f;
        }
    }

    public void NoReverseInput()
    {
        if(brakeInput < 0f)
        {
            brakeInput = 0f;
        }
    }

    void Update()
    {
        //GetInputs();
        AnimateWheels();
        //WheelEffects();
        Debug.Log("The Move value is " + moveInput);
        Debug.Log("The Steer value is " + steerInput);
    }

    void LateUpdate()
    {
        Move();
        Steer();
        Brake();
        Debug.Log("Just steered and moved");
    }

    public void SetSteerInputFromWheelRotation(float steeringWheelRotation)
    {
        // Convert the steering wheel rotation to a value between -1 and 1, assuming 360 degrees is the full range
        steerInput = (steeringWheelRotation / 360.0f) * maxSteerAngle;
    }

    public void MoveInput(float input)
    {
        moveInput = input;
    }

    public void SteerInput(float input)
    {
        steerInput = input;
    }

    /*void GetInputs()
    {
        if (control == ControlMode.VrSteering)
        {
            // PAPA Clamp the rotation angle of the steering wheel input
            //float maxSteerAngleRadians = Mathf.Deg2Rad * maxSteerAngle;
            //float clampedRotation = Mathf.Clamp(VrSteertransform.rotation.z, -maxSteerAngleRadians, maxSteerAngleRadians);
            //steerInput = clampedRotation * Mathf.Rad2Deg * -1; // Convert back to degrees and reverse
            steerInput = (VrSteertransform.rotation.z * -1);

            if (VrSteertransform.rotation.z > maxSteerInput && VrSteertransform.rotation.z < 180)
            {
                VrSteertransform.rotation = Quaternion.Euler(0, 0, maxSteerInput - 1);
            }
            else if (VrSteertransform.rotation.z < minSteerInput && VrSteertransform.rotation.z > -180)
            {
                VrSteertransform.rotation = Quaternion.Euler(23.6f, 0, minSteerInput + 1);
            }

            steerInput = Mathf.Clamp(steerInput, minSteerInput, maxSteerInput);

            float accelerationInput = moveInput;
            // PAPA Acceleration input from VR controller right trigger
            moveInput = Mathf.Clamp01(accelerationInput);


            // PAPA Braking input from VR controller left trigger
            float brakeInput = moveInput;
            if (brakeInput > 0f)
                moveInput = -Mathf.Clamp01(brakeInput); // Negative value for braking*//*



            // steerInput = VrSteertransform.rotation.z * -1;
        }
    }*/

    void Move()
    {
        foreach (var wheel in wheels)
        {
            wheel.wheelCollider.motorTorque = Gear * moveInput * 600 * maxAcceleration * Time.deltaTime;
        }
    }

    void Steer()
    {
        foreach (var wheel in wheels)
        {
            if (wheel.axel == Axel.Front)
            {
                var _steerAngle = steerInput * turnSensitivity * maxSteerAngle;
                wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, _steerAngle, 0.6f);
            }
        }
    }

    void Brake()
    {
        if (Input.GetKey(KeyCode.Space) || brakeInput > 0f)
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = brakeInput * 300 * brakeAcceleration * Time.deltaTime;
            }
        }
        else
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = 0;
            }


        }
    }

    void AnimateWheels()
    {
        foreach (var wheel in wheels)
        {
            Quaternion rot;
            Vector3 pos;
            wheel.wheelCollider.GetWorldPose(out pos, out rot);
            wheel.wheelModel.transform.position = pos;
            wheel.wheelModel.transform.rotation = rot;
        }
    }

    void WheelEffects()
    {
        foreach (var wheel in wheels)
        {
            //var dirtParticleMainSettings = wheel.smokeParticle.main;

            if (Input.GetKey(KeyCode.Space) && wheel.axel == Axel.Rear && wheel.wheelCollider.isGrounded == true && carRb.velocity.magnitude >= 10.0f)
            {
                wheel.wheelEffectObj.GetComponentInChildren<TrailRenderer>().emitting = true;
                wheel.smokeParticle.Emit(1);
            }
            else
            {
                wheel.wheelEffectObj.GetComponentInChildren<TrailRenderer>().emitting = false;
            }
        }
    }

    void OnEnable()
    {
        Debug.Log("Enabling InputActions");
        controls.Enable();
    }
    void OnDisable()
    {
        Debug.Log("Disabling InputActions");
        controls.Disable();
    }
}
















// old

//using UnityEngine;
//using System;
//using System.Collections.Generic;

//public class CarController : MonoBehaviour
//{
//    public enum ControlMode
//    {
//        Keyboard,
//        Buttons,
//        VrSteering
//    };

//    public enum Axel
//    {
//        Front,
//        Rear
//    }

//    [Serializable]
//    public struct Wheel
//    {
//        public GameObject wheelModel;
//        public WheelCollider wheelCollider;
//        public GameObject wheelEffectObj;
//        public ParticleSystem smokeParticle;
//        public Axel axel;
//    }

//    public ControlMode control;

//    public float maxAcceleration = 30.0f;
//    public float brakeAcceleration = 50.0f;

//    public float turnSensitivity = 1.0f;
//    public float maxSteerAngle = 30.0f;

//    public Vector3 _centerOfMass;

//    public List<Wheel> wheels;

//    float moveInput;
//    float steerInput;

//    private Rigidbody carRb;
//    PlayerControls _InputsActions;
//    private bool InputActions;
//    public Transform VrSteertransform;



//    void Start()
//    {
//        carRb = GetComponent<Rigidbody>();
//        carRb.centerOfMass = _centerOfMass;
//    }

//    void Awake()
//    {
//        if (InputActions == true)
//        {
//            _InputsActions = new PlayerControls();
//            Debug.Log("Drive controls are set.");

//            _InputsActions.Other.Drive.performed += ctx => moveInput = ctx.ReadValue<float>();
//            _InputsActions.Other.Drive.canceled += ctx => moveInput = 0f;
//            _InputsActions.Other.Steer.performed += ctx => steerInput = ctx.ReadValue<float>();
//            _InputsActions.Other.Steer.canceled += ctx => steerInput = 0f;
//        }
//    }

//    void Update()
//    {
//        GetInputs();
//        AnimateWheels();
//        WheelEffects();
//    }

//    void LateUpdate()
//    {
//        Move();
//        Steer();
//        Brake();
//    }

//    public void SetSteerInputFromWheelRotation(float steeringWheelRotation)
//    {
//        // Convert the steering wheel rotation to a value between -1 and 1, assuming 360 degrees is the full range
//        steerInput = (steeringWheelRotation / 360.0f) * maxSteerAngle;
//    }

//    public void MoveInput(float input)
//    {
//        moveInput = input;
//    }

//    public void SteerInput(float input)
//    {
//        steerInput = input;
//    }

//    void GetInputs()
//    {
//        if (control == ControlMode.Keyboard)
//        {
//            moveInput = Input.GetAxis("Vertical");
//            steerInput = Input.GetAxis("Horizontal");
//        }
//        else if (control == ControlMode.VrSteering)
//        {
//            // PAPA 
//            steerInput = VrSteertransform.rotation.z * -1;
//            // PAPA Acceleration input from VR controller right trigger
//            float accelerationInput = Input.GetAxis("XRI_Right_Trigger"); // Replace "VR_Accelerate" with your input name
//            moveInput = Mathf.Clamp01(accelerationInput);

//            // PAPA Braking input from VR controller left trigger
//            float brakeInput = Input.GetAxis("XRI_Left_Trigger"); // Replace "VR_Brake" with your input name
//            if (brakeInput > 0f)
//                moveInput = -Mathf.Clamp01(brakeInput); // Negative value for braking


//        }
//    }

//    void Move()
//    {
//        foreach (var wheel in wheels)
//        {
//            wheel.wheelCollider.motorTorque = moveInput * 600 * maxAcceleration * Time.deltaTime;
//        }
//    }

//    void Steer()
//    {
//        foreach (var wheel in wheels)
//        {
//            if (wheel.axel == Axel.Front)
//            {
//                var _steerAngle = steerInput * turnSensitivity * maxSteerAngle;
//                wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, _steerAngle, 0.6f);
//            }
//        }
//    }

//    void Brake()
//    {
//        if (Input.GetKey(KeyCode.Space) || moveInput == 0)
//        {
//            foreach (var wheel in wheels)
//            {
//                wheel.wheelCollider.brakeTorque = 300 * brakeAcceleration * Time.deltaTime;
//            }


//        }
//        else
//        {
//            foreach (var wheel in wheels)
//            {
//                wheel.wheelCollider.brakeTorque = 0;
//            }


//        }
//    }

//    void AnimateWheels()
//    {
//        foreach (var wheel in wheels)
//        {
//            Quaternion rot;
//            Vector3 pos;
//            wheel.wheelCollider.GetWorldPose(out pos, out rot);
//            wheel.wheelModel.transform.position = pos;
//            wheel.wheelModel.transform.rotation = rot;
//        }
//    }

//    void WheelEffects()
//    {
//        foreach (var wheel in wheels)
//        {
//            //var dirtParticleMainSettings = wheel.smokeParticle.main;

//            if (Input.GetKey(KeyCode.Space) && wheel.axel == Axel.Rear && wheel.wheelCollider.isGrounded == true && carRb.velocity.magnitude >= 10.0f)
//            {
//                wheel.wheelEffectObj.GetComponentInChildren<TrailRenderer>().emitting = true;
//                wheel.smokeParticle.Emit(1);
//            }
//            else
//            {
//                wheel.wheelEffectObj.GetComponentInChildren<TrailRenderer>().emitting = false;
//            }
//        }
//    }
//}