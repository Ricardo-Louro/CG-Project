using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    private PlayerController playerController;

    void Start()
    {
        playerController = transform.parent.GetComponent<PlayerController>();
    }

    private void OnTriggerEnter()
    {
        playerController.Grounded = true;
    }

    private void OnTriggerExit()
    {
        playerController.Grounded = false;
    }
}