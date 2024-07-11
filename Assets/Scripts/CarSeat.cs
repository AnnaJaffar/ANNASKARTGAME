//using System.Collections;
//using System.Collections.Generic;
//using Unity.VRTemplate;
//using UnityEngine;
//using UnityEngine.InputSystem;
//using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

//public class CarEnter : MonoBehaviour
//{
//    public GameObject car;
//    public GameObject driverPosition;

//    private bool inCar = false;
//    private PlayerInput playerInput; // Reference to the PlayerInput component

//    private void OnTriggerEnter(Collider other)
//    {
//        if (other.CompareTag("Player") && !inCar)
//        {
//            // Move the player to the driver position
//            other.transform.position = driverPosition.transform.position;
//            other.transform.rotation = driverPosition.transform.rotation;

//            // Make the player a child of the car
//            other.transform.parent = car.transform;

//            // Disable player's character controller to prevent movement
//            CharacterController characterController = other.GetComponent<CharacterController>();
//            if (characterController != null)
//            {
//                characterController.enabled = false;
//            }

//            // Disable the player's input
//            playerInput = other.GetComponent<PlayerInput>();
//            if (playerInput != null)
//            {
//                playerInput.enabled = false;
//            }
//            // My attempt at disabling player left stick input. This snippet is found in the LeftHandSmoothAnchor object
//            SmoothTrackingFollow smoothTrackingFollow = other.GetComponent<SmoothTrackingFollow>();
//            if (smoothTrackingFollow != null)
//            {
//                smoothTrackingFollow.enabled = false;
//            }
//            // Set inCar flag to true
//            inCar = true;
//        }
//    }


//    private void OnTriggerExit(Collider other)
//    {
//        if (other.CompareTag("Player") && inCar)
//        {
//            // Unparent the player from the car
//            other.transform.parent = null;

//            // Enable player's character controller
//            CharacterController characterController = other.GetComponent<CharacterController>();
//            if (characterController != null)
//            {
//                characterController.enabled = true;
//            }

//            // Enable the player's input
//            if (playerInput != null)
//            {
//                playerInput.enabled = true;
//            }

//            // Reset inCar flag
//            inCar = false;
//        }
//    }
//    void DisableLocomotionActions()
//    {
//        // Disable locomotion actions (assuming player uses XR interaction toolkit)
//        ActionBasedControllerManager controllerManager = FindFirstObjectByType<ActionBasedControllerManager>();
//        if (controllerManager != null)
//        {
//            controllerManager.DisableLocomotionActions();
//        }
//    }
//}



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class CarEnter : MonoBehaviour
{
    [Tooltip("Needs To be the whole car model, NOT just the body")]
    public GameObject car;

    [Tooltip("Make an empty gameObject and move it to where you want your driver to sit assign that gameobject to this.")]
    public GameObject driverPosition;

    [Tooltip("Do not change this always keep it as false unless you know what you are doing and you start the game already in the kart.")]
    public bool inCar = false; //PAPA changed private to public
    public UnityEvent OnTriggerEnterEvent;
    public UnityEvent OnTriggerExitEvent;
    private LockObject lockObject;
    public float carPlayerHeight = -0.35f;
    private DriverCameraView driverCameraView;
    PlayerControl controls;
    public Collider player;

    void Awake()
    {
        controls = new PlayerControl();

        controls.Other.CarExit.performed += ctx => ExitCar(player);
    }

    void Start()
    {
        driverCameraView = FindAnyObjectByType<DriverCameraView>();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !inCar)
        {
            Debug.Log("Should be getting into car right now.");

            // Move the player to the driver position
            other.transform.SetPositionAndRotation(driverPosition.transform.position, driverPosition.transform.rotation);

            // Make the player a child of the car
            other.transform.parent = car.transform;

            driverCameraView.LowerCamera();

            lockObject = FindAnyObjectByType<LockObject>();

            lockObject.LockPosition();
            lockObject.LockRotation();
                         
            // PAPA Disable player's character controller to prevent movement
            CharacterController characterController = other.GetComponent<CharacterController>();
            if (characterController != null)
            {
                characterController.enabled = false;
            }

            //disabling collider
            other.enabled = false;

            // Set inCar flag to true
            inCar = true;

            // Trigger UnityEvent
            OnTriggerEnterEvent.Invoke();
        }
    }

    private void ExitCar(Collider other)
    {
        lockObject = FindAnyObjectByType<LockObject>();

        lockObject.UnlockPosition();
        lockObject.UnlockRotation();

        // Unparent the player from the car
        other.transform.parent = null;

        driverCameraView.RaiseCamera();

        inCar = false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && inCar)
        {
            // PAPA Enable player's character controller
            CharacterController characterController = other.GetComponent<CharacterController>();
            if (characterController = null)
            {
                characterController.enabled = true;
            }

            // Reset inCar flag
            inCar = false;

            OnTriggerExitEvent.Invoke();
        }
     
        }

    }

