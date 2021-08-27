using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    Rigidbody rb;
    CapsuleCollider cc;

    public Transform head;

    public float cameraSensivityX = 2.5f;
    public float cameraSensivityY = 2.5f;
    public bool canLook = true;

    public float moveSpeed = 5;
    public bool canMove = true;

    public float jumpForce = 5;
    public float groundingRaycastLength = 0.1f;
    public LayerMask groundingDetection = ~0;
    public bool canJump = true;

    Vector3 movementValues;
    Vector2 cameraValues;

    public bool IsGrounded
    {
        get
        {
            Vector3 rayOrigin = transform.position + transform.up;
            Vector3 rayDirection = -transform.up;
            RaycastHit rh;
            if (Physics.SphereCast(rayOrigin, cc.radius, rayDirection, out rh, 1 + groundingRaycastLength, groundingDetection))
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
        
        


        if (Input.GetButton("Jump") && IsGrounded && canJump)
        {
            willJump = true;
        }

        if (canMove)
        {
            movementValues = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        }
        
        
    }

    private void FixedUpdate()
    {
        movementValues = transform.rotation * movementValues;
        rb.MovePosition(transform.position + movementValues * moveSpeed * Time.fixedDeltaTime);

        if (willJump == true)
        {
            willJump = false;
            rb.AddForce(transform.up * jumpForce);
        }
    }
}
