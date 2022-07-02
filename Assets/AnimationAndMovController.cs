using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//so that unity recognises the callbacks ctx passed to the movementinput
using UnityEngine.InputSystem;

public class AnimationAndMovController : MonoBehaviour
{

    public Transform cam;

    public float speed;
    //here we are creadting three vars for the animation
    //vector2 currentMovementInput stores the input axis of the player
    //vector3 store the current position of the player


    PlayerInput playerInput; 
    CharacterController characterController;
    Animator animator;

    Vector2 currentMovementInput;
    Vector3 currentMovement;
    Vector3 currentRunMovement;
    Vector3 moveDir;

    
    bool isMovementPressed;
    bool isRunPressed;

    // float rotationFactor = 15.0f;

    //variables to rotate the player -> BRACKEYS 3rd person controller
    float turnSmoothtime = 0.1f;
    float turnSmoothVelocity ;


    float runMultiplier = 3.5f;

    // int walkHash ;
    // int runHash ;

    //runs before start function
    void Awake(){
        playerInput = new PlayerInput();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        // walkHash = Animator.StringToHash("Walking");
        // runHash = Animator.StringToHash("run");

        //now instead of writing the logic three times we pass the callback ctx to the movementInput() function
        playerInput.CharacterControls.Move.started += movementInput;
        playerInput.CharacterControls.Move.canceled += movementInput;
        playerInput.CharacterControls.Move.performed += movementInput;
        playerInput.CharacterControls.Run.started += handleRun;
        playerInput.CharacterControls.Run.canceled += handleRun;
    }

    void handleRun(InputAction.CallbackContext ctx){
        isRunPressed = ctx.ReadValueAsButton();
    }

    //we are going to handle rotations with quaternions
    void handleRotation(){
        Vector3 positionToLookAt;

        positionToLookAt.x = isRunPressed ? currentRunMovement.x : currentMovement.x;
        positionToLookAt.y = 0.0f ;
        positionToLookAt.z = isRunPressed ? currentRunMovement.z : currentMovement.z;

        Vector3 direction = new Vector3(positionToLookAt.x, 0.0f, positionToLookAt.z).normalized;

        // Quaternion currentRotation = transform.rotation;

        //we take the current rotation and the target rotation and slerp them *FYI : Im still not sure how slerp works
        if(isMovementPressed){
            // Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            // transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationFactor * Time.deltaTime);


            //I HAVE NO IDEA WHAT IS HAPPENING HERE. I JUST COPIED SOME CODE FROM BRACKEYS 3rd PERSON CONTROLLER to rotate the player
            //I am not sure if this is the best way to do it
            
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothtime);
            transform.rotation = Quaternion.Euler(0.0f, angle, 0.0f);

            //here Im storing the rotation of the player including the camera rotation and calling the moveDir in the Update() function
            moveDir = Quaternion.Euler(0.0f, targetAngle, 0.0f) * Vector3.forward;
            
        }

    }

    //we are passing the callback ctx to this function so that we dont have to call the function everytime we start, cancel or perform the movement
    void movementInput(InputAction.CallbackContext ctx){
        //we are setting the movement input to the axis of the player
        currentMovementInput = ctx.ReadValue<Vector2>();
        currentMovement.x = currentMovementInput.x;
        //we are setting the z axis to the y axis of the player because we move y axis on keyboard or joystick but in game we move in z axis
        currentMovement.z = currentMovementInput.y;


        //now we are setting the run movement to the current movement
        currentRunMovement.x = currentMovementInput.x *  runMultiplier;
        currentRunMovement.z = currentMovementInput.y * runMultiplier;

        isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;

    }

    void handleAnimation(){
        bool walk = animator.GetBool("walking");
        bool run = animator.GetBool("run");

        if(isMovementPressed && !walk){
            animator.SetBool("walking", true);
        }
        else if(!isMovementPressed && walk){
            animator.SetBool("walking", false);
        }

        if((isMovementPressed && isRunPressed) && !run){
            animator.SetBool("run", true);
        }
        else if((!isMovementPressed || !isRunPressed)&& run){
            animator.SetBool("run", false);
        }
    }

    void handleGravity(){
        //we are setting the gravity to -9.8f because we are moving in y axis
        if(characterController.isGrounded){
            float groundGravity = -0.05f;
            currentMovement.y = groundGravity;
            currentRunMovement.y = groundGravity;

        }
        else{
            float gravity = -9.8f;
            currentMovement.y += gravity * Time.deltaTime;
            currentRunMovement.y += gravity * Time.deltaTime;
        }
    }

    // Update is called once per frame
    void Update()
    {
        handleAnimation();
        handleRotation();
        handleGravity();
        
        if(isRunPressed){

            characterController.Move(moveDir * runMultiplier * Time.deltaTime);
        }
       
        if(isMovementPressed){
         characterController.Move(moveDir * Time.deltaTime);
        }
       
    }

    //we are checking if the player script gets enabled or disabled and accordingly we are enabling or disabling the player input
    void OnEnable(){
        playerInput.CharacterControls.Enable();
    }

    void OnDisable(){
        playerInput.CharacterControls.Disable();
    }
}
