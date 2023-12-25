using UnityEngine;

//Class will check if the player is grounded or not and return this information to the PlayerController
public class GroundChecker : MonoBehaviour
{
    //Declares a variable containing the PlayerController so that it can be referenced throughout the script
    private PlayerController playerController;

    //Runs the first frame when this object is active
    void Start()
    {
        //Assigns the PlayerController to the relevant variable
        playerController = transform.parent.GetComponent<PlayerController>();
    }

    //When the Collider enters in contact with something...
    private void OnTriggerEnter()
    {
        //...set the player as grounded
        playerController.Grounded = true;
    }

    //When the collider exits contact with something...
    private void OnTriggerExit()
    {
        //...set the player as no longer grounded
        playerController.Grounded = false;
    }
}