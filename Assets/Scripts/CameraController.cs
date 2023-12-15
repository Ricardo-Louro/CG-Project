using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject     player;
    [SerializeField] private GameObject     effectCamera;
    [SerializeField] private float          mouseSensitivityX;
    [SerializeField] private float          mouseSensitivityY;
    private float                           mouseX = 0f;
    private float                           mouseY = 0f;
 
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        ReceiveInput();
        UpdateCamPosition();
        UpdateCamsRotations();
    }

    private void UpdateCamPosition()
    {
        Vector3 playerPos = player.transform.position;
        transform.position = new Vector3(playerPos.x,
                                         playerPos.y + 0.5f,
                                         playerPos.z);
    }

    private void UpdateCamsRotations()
    {   
        transform.eulerAngles = new Vector3(mouseY, mouseX, 0);
        effectCamera.transform.eulerAngles = new Vector3(mouseY, mouseX, 0);
    }

    private void ReceiveInput()
    {
        mouseX += mouseSensitivityX * Input.GetAxis("Mouse X");
        mouseY -= mouseSensitivityY * Input.GetAxis("Mouse Y");

        mouseY = Mathf.Clamp(mouseY, -88f, 88f);
    }    
}