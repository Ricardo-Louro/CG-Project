using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameObject     mainCamera;
    [SerializeField] private GameObject     elementHUD;
    [SerializeField] private float          moveSpeed;
    [SerializeField] private float          jumpSpeed;
    private Rigidbody                       rb;
    private float                           horizontalInput;
    private float                           verticalInput;
    public bool                             Grounded;
    private bool                            jumpQueued = false;

    public bool                            pointingCam {get; private set;} = false;
    
    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        ReceiveInput();
        Rotate();
    }

    void FixedUpdate()
    {
        MoveAndJump();
    }

    private void ReceiveInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if(Input.GetKeyDown(KeyCode.Space) && Grounded)
        {
            jumpQueued = true;
        }
        if(Input.GetKeyDown(KeyCode.Mouse1))
        {
            pointingCam = true;
            ActivationHUD(true, elementHUD);
        }
        if(Input.GetKeyUp(KeyCode.Mouse1))
        {
            pointingCam = false;
            ActivationHUD(false, elementHUD);
        }
    }

    private void Rotate()
    {
        Vector3 camRotation = mainCamera.transform.eulerAngles;
        transform.eulerAngles = new Vector3 (0, camRotation.y, 0);
    }
    
    private void MoveAndJump()
    {
        if(Grounded)
        {
            float verticalSpeed = rb.velocity.y;

            Vector3 moveDirection = transform.forward * verticalInput +
                                    transform.right * horizontalInput;

            rb.velocity = moveDirection.normalized * moveSpeed * Time.deltaTime;
            rb.velocity = new Vector3(rb.velocity.x, verticalSpeed, rb.velocity.z);

            if(jumpQueued)
            {
                jumpQueued = false;
                rb.velocity = new Vector3(rb.velocity.x, jumpSpeed, rb.velocity.z);
            }
        }
    }

    private void ActivationHUD(bool activation, GameObject HUD)
    {
        HUD.SetActive(activation);
    }
}