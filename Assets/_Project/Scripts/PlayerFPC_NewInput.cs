using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerFPC_NewInput : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float crouchSpeed = 2f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;

    [Header("Mouse")]
    public float mouseSensitivity = 2.5f;
    public Transform cameraPivot;

    [Header("Crouch")]
    public float crouchHeight = 1f;
    public float crouchCameraOffset = 0.2f;

    private Rigidbody rb;
    private CapsuleCollider col;
    private PlayerInputActions input;

    private Vector2 moveInput;
    private Vector2 lookInput;

    private bool runPressed;
    private bool crouchPressed;
    private bool jumpPressed;

    private float xRotation;
    private float originalHeight;
    private Vector3 originalCenter;
    private Vector3 velocity;

    private void Awake()
    {
        input = new PlayerInputActions();

        input.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        input.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        input.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        input.Player.Run.performed += _ => runPressed = true;
        input.Player.Run.canceled += _ => runPressed = false;

        input.Player.Crouch.performed += _ => StartCrouch();
        input.Player.Crouch.canceled += _ => StopCrouch();

        input.Player.Jump.performed += _ => jumpPressed = true;
    }

    private void OnEnable() => input.Enable();
    private void OnDisable() => input.Disable();

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();

        rb.freezeRotation = true;

        originalHeight = col.height;
        originalCenter = col.center;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        Look();
    }

    private void FixedUpdate()
    {
        Move();
        ApplyGravity();
        Jump();
    }

    // -------- MOVE --------
    private void Move()
    {
        float speed = runPressed && !crouchPressed ? runSpeed : walkSpeed;
        if (crouchPressed) speed = crouchSpeed;

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        Vector3 targetVelocity = move * speed;
        targetVelocity.y = rb.linearVelocity.y; // �������� ����������� ��������
        rb.linearVelocity = targetVelocity;
    }

    // -------- LOOK --------
    private void Look()
    {
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    // -------- JUMP --------
    private void Jump()
    {
        if (!jumpPressed) return;

        if (IsGrounded() && !crouchPressed)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        jumpPressed = false;
    }

    // -------- GRAVITY --------
    private void ApplyGravity()
    {
        if (!IsGrounded())
        {
            rb.AddForce(Vector3.up * gravity, ForceMode.Acceleration);
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, col.bounds.extents.y + 0.1f);
    }

    // -------- CROUCH --------
    private void StartCrouch()
    {
        crouchPressed = true;
        col.height = crouchHeight;
        col.center = new Vector3(0, crouchHeight / 2f, 0);
        cameraPivot.localPosition = new Vector3(0, crouchHeight - crouchCameraOffset, 0);
    }

    private void StopCrouch()
    {
        crouchPressed = false;
        col.height = originalHeight;
        col.center = originalCenter;
        cameraPivot.localPosition = new Vector3(0, originalHeight / 2f, 0);
    }
}
