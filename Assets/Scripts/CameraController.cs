//Declare relevant namespaces
using UnityEngine;

//Controls the cameras' movement and rotation so that the player can see
public class CameraController : MonoBehaviour
{
    //Declare variables which will store both the player and the effect camera so they can be referred to
    [SerializeField] private GameObject     player;
    [SerializeField] private GameObject     effectCamera;
    
    //Declare the sensitivity value for the mouse movement of the camera so it can be easily accessed and customized
    [SerializeField] private float          mouseSensitivityX;
    [SerializeField] private float          mouseSensitivityY;

    //Declare variables for the rotation so it can be constantly updated to remain consistent from frame to frame
    private float                           mouseX = 0f;
    private float                           mouseY = 0f;

    //Runs at the first frame when this object is active
    private void Start()
    {
        //Lock the cursor to the application screen to prevent accidental inputs on setups with multiple monitors 
        Cursor.lockState = CursorLockMode.Locked;
    }

    //Runs every frame after the first one while this object is active
    void Update()
    {
        //Receives the player's inputs
        ReceiveInput();
        //Updates the position of the camera
        UpdateCamPosition();
        //Update the rotation of the cameras
        UpdateCamsRotations();
    }

    //Receives the player's inputs and stores them to be used within this script
    private void ReceiveInput()
    {
        //Adds the movement of the mouse to the variables which control the player's rotation
        mouseX += mouseSensitivityX * Input.GetAxis("Mouse X");
        mouseY -= mouseSensitivityY * Input.GetAxis("Mouse Y");

        //Clamp the vertical rotation of the player to be within acceptable values
        mouseY = Mathf.Clamp(mouseY, -88f, 88f);
    }   

    //Updates the camera position so that it is continuously synched with the player's position
    private void UpdateCamPosition()
    {
        //Store the player's position within a local variable
        Vector3 playerPos = player.transform.position;
        
        //Update the camera's position by the local variable with a slight offset on the vertical coordinate to be at eye level
        transform.position = new Vector3(playerPos.x,
                                         playerPos.y + 0.5f,
                                         playerPos.z);
    }

    //Update the camera's rotation depending on the variables previously updated by the player's input
    private void UpdateCamsRotations()
    {   
        //Update the rotation of the main camera based on the variables previously updated by the player's input 
        transform.eulerAngles = new Vector3(mouseY, mouseX, 0);
        //Update the rotation of the effect camera based on the variables previously updated by the player's input
        effectCamera.transform.eulerAngles = new Vector3(mouseY, mouseX, 0);
    } 
}