using UnityEngine;

public class DesktopXRController : MonoBehaviour
{
    [Header("Mode")]
    [Tooltip("勾选 = 鼠标键盘模式；不勾选 = VR模式")]
    public bool desktopMode = true;

    [Header("Camera")]
    public Transform headCamera;

    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Gravity")]
    public float gravity = -9.81f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 0.6f;
    public float maxLookAngle = 80f;

    private CharacterController characterController;

    private float verticalVelocity = 0f;
    private float pitch = 0f;

    void Start()
    {
        characterController =
            GetComponent<CharacterController>();

        if (desktopMode)
        {
            Cursor.lockState =
                CursorLockMode.Locked;

            Cursor.visible = false;
        }
    }

    void Update()
    {
        if (!desktopMode)
            return;

        HandleMovement();
        HandleMouseLook();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState =
                CursorLockMode.None;

            Cursor.visible = true;
        }

        if (Input.GetMouseButtonDown(0) &&
            Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState =
                CursorLockMode.Locked;

            Cursor.visible = false;
        }
    }

    private void HandleMovement()
    {
        if (characterController == null)
            return;

        if (headCamera == null)
            return;


        float horizontal =
            Input.GetAxisRaw("Horizontal");

        float vertical =
            Input.GetAxisRaw("Vertical");


        Vector3 forward =
            headCamera.forward;

        Vector3 right =
            headCamera.right;


        // 电脑模式只沿地面方向移动
        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();


        Vector3 moveDirection =
            forward * vertical +
            right * horizontal;


        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }


        // 地面检测
        if (characterController.isGrounded)
        {
            if (verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }
        }
        else
        {
            verticalVelocity +=
                gravity * Time.deltaTime;
        }


        Vector3 finalMove =
            moveDirection * moveSpeed;

        finalMove.y =
            verticalVelocity;


        characterController.Move(
            finalMove * Time.deltaTime
        );
    }

    private void HandleMouseLook()
    {
        if (headCamera == null)
            return;


        float mouseX =
            Input.GetAxis("Mouse X") *
            mouseSensitivity;

        float mouseY =
            Input.GetAxis("Mouse Y") *
            mouseSensitivity;


        // 左右转整个 XR Origin
        transform.Rotate(
            0f,
            mouseX,
            0f,
            Space.World
        );


        // 上下看
        pitch -= mouseY;

        pitch = Mathf.Clamp(
            pitch,
            -maxLookAngle,
            maxLookAngle
        );


        Vector3 currentRotation =
            headCamera.localEulerAngles;

        headCamera.localEulerAngles =
            new Vector3(
                pitch,
                currentRotation.y,
                currentRotation.z
            );
    }
}