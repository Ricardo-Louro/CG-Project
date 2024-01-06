//Declare relevant namespaces
using UnityEngine;

//Receives the inputs from the player and handles the movement of the player's character
public class PlayerController : MonoBehaviour
{
    //Declare references to other GameObjects/Components so we may refer to them in this script
    [SerializeField] private GameObject     mainCamera;
    [SerializeField] private GameObject     effectCamera;
    [SerializeField] private GameObject     elementHUD;
    private Rigidbody                       rb;
    
    //Set serialized field variables for ease of access and modification
    [SerializeField] private float          moveSpeed;
    [SerializeField] private float          jumpSpeed;

    //Initialize variables which will be necessary for this script's operations
    private float                           horizontalInput;
    private float                           verticalInput;
    public bool                             Grounded;
    private bool                            jumpQueued = false;
    public bool                            pointingCam {get; private set;} = false;
    
    //Runs the first frame where this script is active
    private void Start()
    {
        //Assign the rigidbody attached to the player to the correct variable
        rb = gameObject.GetComponent<Rigidbody>();
    }

    //Runs every frame following the first when this script is active
    private void Update()
    {
        //Receives the player's input and proceeds with the relevant operations
        ReceiveInput();
        //Rotates the player's character according to the current orientation of the camera
        Rotate();
    }

    //Runs during every physics step
    void FixedUpdate()
    {
        //Moves the player according to the inputs previously received
        MoveAndJump();
    }

    //Receives the player's input and proceeds with the relevant operations
    private void ReceiveInput()
    {
        //Receive the player's horizontal input
        horizontalInput = Input.GetAxisRaw("Horizontal");
        //Receive the player's vertical input
        verticalInput = Input.GetAxisRaw("Vertical");

        //If the player has pressed the spacebar and is currently grounded...
        if(Input.GetKeyDown(KeyCode.Space) && Grounded)
        {
            //...set the jump flag to true
            jumpQueued = true;
        }
        //If the player pressed down the right mouse button...
        if(Input.GetKeyDown(KeyCode.Mouse1))
        {
            //...set the pointing camera flag as true
            pointingCam = true;
            //...activate the pointing camera HUD elements
            ActivateObject(true, elementHUD);
            //...activate the effect camera
            ActivateObject(true, effectCamera);
        }
        //If the player released the right mouse button...
        else if(Input.GetKeyUp(KeyCode.Mouse1))
        {
            //...set the pointing camera flag as false
            pointingCam = false;
            //...deactivate the pointing camera HUD elements
            ActivateObject(false, elementHUD);
            //...deactivate the effect camera
            ActivateObject(false, effectCamera);
        }
    }

    //Rotates the player's character according to the current orientation of the camera
    private void Rotate()
    {
        //Stores the main camera's rotation
        Vector3 camRotation = mainCamera.transform.eulerAngles;
        //Assigns the rotation of the main camera around the y axis as the rotation for the player's character
        transform.eulerAngles = new Vector3 (0, camRotation.y, 0);
    }
    
    //Moves the player according to the inputs previously received
    private void MoveAndJump()
    {
        //If the player is currently on a solid surface...
        if(Grounded)
        {
            //Store the player character's current velocity in the y axis
            float verticalSpeed = rb.velocity.y;

            //Calculate the player's movement direction vector
            Vector3 moveDirection = transform.forward * verticalInput +
                                    transform.right * horizontalInput;


            //Assign the new player's velocity according to the new movement direction vector
            rb.velocity = moveDirection.normalized * moveSpeed * Time.deltaTime;
            //Correct the player character's velocity in the y axis (is always 0 according to the movement direction vector) to the previously stored velocity
            rb.velocity = new Vector3(rb.velocity.x, verticalSpeed, rb.velocity.z);

            //If the jump flag was true...
            if(jumpQueued)
            {
                //...set it as false
                jumpQueued = false;
                //...set the player character's velocity in the y axis to the jump speed value
                rb.velocity = new Vector3(rb.velocity.x, jumpSpeed, rb.velocity.z);
            }
        }
    }

    //Activates and deactivates elements
    private void ActivateObject(bool activation, GameObject obj)
    {
        //Set the HUD elements' state as active/disabled according to the provided bool
        obj.SetActive(activation);
    }
}