using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine.Animations;

public class CustomPlayerController : NetworkBehaviour
{
    [Header("Base setup")]
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;

    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    [HideInInspector]
    public bool canMove = true;

    [SerializeField]
    private float cameraYOffset = 0.4f;
    private Camera playerCamera;

    [SerializeField] private Transform cameraRotation;


    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsOwner)
        {
            
            
            // AssignCameras();
            // playerCamera = Camera.main;
            // playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y + cameraYOffset, transform.position.z);
            // playerCamera.transform.SetParent(transform);
        }
        else
        {
            // var players = FindObjectsOfType<CustomPlayerController>();
            // if (players.Length <= 1)
            //     return;
            
            // AssignCameras();
            // gameObject.GetComponent<CustomPlayerController>().enabled = false;
        }
    }

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private bool done;
    void Update()
    {
        if (!done)
            CheckForOtherClients();
        bool isRunning = false;

        // Press Left Shift to run
        isRunning = Input.GetKey(KeyCode.LeftShift);

        // We are grounded, so recalculate move direction based on axis
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpSpeed;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);

        // Player and Camera rotation
        if (canMove && playerCamera != null)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            cameraRotation.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }

    private void CheckForOtherClients()
    {
        var players = FindObjectsOfType<CustomPlayerController>();
        if (players.Length <= 1)
            return;
        
        AssignCameras();
    }
    
    public void AssignCameras()
    {
        var players = FindObjectsOfType<CustomPlayerController>();
        
        playerCamera = Camera.main;

        foreach (var playerController in players)
        {
            if (playerController != this)
            {
                playerCamera.transform.position = new Vector3(playerController.transform.position.x, playerController.transform.position.y + cameraYOffset, playerController.transform.position.z);
                playerCamera.transform.SetParent(playerController.transform);
                
                var rotationOfCamera = playerController.transform.GetChild(2);
                var rotationConstraint = playerCamera.GetComponentInChildren<RotationConstraint>();
                
                rotationConstraint.AddSource(new ConstraintSource
                {
                    sourceTransform = rotationOfCamera,
                    weight = 1
                });
                rotationConstraint.constraintActive = true;
                
                playerController.enabled = false;
            }
        }
    }
}