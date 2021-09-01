using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FirstPersonController : MonoBehaviour
{
    Rigidbody rb;
    CapsuleCollider cc;
    public Transform head;

    [Header("Camera")]
    public float cameraSensivityX = 2.5f;
    public float cameraSensivityY = 2.5f;
    public bool canLook = true;
    Vector2 cameraValues;

    [Header("Movement")]
    public float moveSpeed = 5;
    public bool canMove = true;
    Vector3 movementValues;

    [Header("Jumping")]
    public float jumpForce = 5;
    public float groundingRaycastLength = 0.1f;
    public float groundingRaycastRadius = 0.3f;
    public LayerMask groundingDetection = ~0;
    public bool canJump = true;

    [Header("Flying")]
    public float verticalMovementSpeed = 20;
    

    public bool IsGrounded
    {
        get
        {
            Vector3 rayOrigin = transform.position + transform.up;
            Vector3 rayDirection = -transform.up;
            RaycastHit rh;
            if (Physics.SphereCast(rayOrigin, groundingRaycastRadius, rayDirection, out rh, 1 + groundingRaycastLength, groundingDetection))
            {
                return true;
            }
            return false;
        }
    }

    bool willJump;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cc = GetComponent<CapsuleCollider>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (canLook)
        {
            cameraValues.x = Input.GetAxis("Mouse X") * cameraSensivityX * Time.deltaTime;
            cameraValues.y -= Input.GetAxis("Mouse Y") * cameraSensivityY * Time.deltaTime;
            cameraValues.y = Mathf.Clamp(cameraValues.y, -90, 90);
            transform.RotateAround(transform.position, transform.up, cameraValues.x);
            head.transform.localRotation = Quaternion.Euler(cameraValues.y, 0, 0);
        }
        
        

        /*
        if (Input.GetButton("Jump") && IsGrounded && canJump)
        {
            willJump = true;
        }
        */
        if (canMove)
        {
            float vertical = 0;
            if (Input.GetKey(KeyCode.Space))
            {
                vertical += 1;
            }
            if (Input.GetKey(KeyCode.LeftShift))
            {
                vertical -= 1;
            }
            movementValues = new Vector3(Input.GetAxis("Horizontal") * moveSpeed, vertical * verticalMovementSpeed, Input.GetAxis("Vertical") * moveSpeed);
        }
        
        
    }

    private void FixedUpdate()
    {
        movementValues = transform.rotation * movementValues;
        rb.MovePosition(transform.position + movementValues * Time.fixedDeltaTime);
        /*
        if (willJump == true)
        {
            willJump = false;
            rb.AddForce(transform.up * jumpForce);
        }
        */
    }
}
