using System;
using SVGTech.Editor.Utils;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovementController : MonoBehaviour
{
    Rigidbody body = null;

    [SerializeField]
    Rotator m_cameraRotator, m_characterRotator;

    public Vector3 Velocity => body.velocity;
    public Vector3 AngularVelocity => angularVelocity;

    [SerializeField, Range(0, 100)]
    float maxFwdSpeed, maxSideSpeed;

    [SerializeField]
    KeyCode runKey;

    [SerializeField]
    float runSpeedMultiplier;

    [SerializeField, Range(0, 100)]
    float maxFwdAcceleration, maxFwdAirAcceleration = 1;

    [SerializeField, Range(0, 100)]
    float maxSideAcceleration, maxSideAirAcceleration = 1;

    [SerializeField, Range(0, 10)]
    float jumpHeight = 2;
    public float JumpHeight => jumpHeight;

    [SerializeField, Range(0, 5)]
    int maxAirJumps = 0;

    [SerializeField, Range(0, 90)]
    float maxGroundAngle = 25;

    Vector3 velocity, desiredVelocity;

    Vector3 lastRotation;
    [SerializeField, ReadOnly]
    Vector3 angularVelocity;

    [SerializeField, ReadOnly]
    bool desiredJump;
    int jumpPhase;

    [SerializeField, ReadOnly]
    bool desiredRun = false;
    public bool IsRunning => desiredRun;
    public event Action OnStartRunning;
    public event Action OnEndRunning;

    [SerializeField, ReadOnly]
    float runMultiplierStrength = 0;
    public float RunMultiplierStrength { get => runMultiplierStrength; set => runMultiplierStrength = Mathf.Clamp01(value); }

    int groundContactCount;
    public bool OnGround => groundContactCount > 0;
    public event Action OnJump;
    public event Action OnAground;
    float minGroundDotProduct;

    Vector3 contactNormal;

    void OnValidate()
    {
        // This is the minimum Y of normal, so lesser the angle more the Y of normal up to 1 and vice-versa
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
    }

    void OnEnable()
    {
        m_cameraRotator.enabled = true;
        m_characterRotator.enabled = true;
    }

    private void OnDisable()
    {
        m_cameraRotator.enabled = false;
        m_characterRotator.enabled = false;
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        OnValidate();
    }

    void Update()
    {
        Vector2 playerInput;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1);

        if (Input.GetKeyDown(runKey))
        {
            desiredRun = true;
            OnStartRunning?.Invoke();
        }
        if (Input.GetKeyUp(runKey))
        {
            desiredRun = false;
            OnEndRunning?.Invoke();
        }

        float v = GetRunMultiplier();
        // Debug.LogError(v);
        desiredVelocity = new Vector3(playerInput.x * maxSideSpeed, 0, playerInput.y * maxFwdSpeed * v);

        desiredJump |= Input.GetButtonDown("Jump");

        angularVelocity = transform.eulerAngles - lastRotation;
        lastRotation = transform.eulerAngles;
    }

    void FixedUpdate()
    {
        UpdateState();

        AdjustVelocity();
        // float acceleration = onGround ? maxAcceleration : maxAirAcceleration;
        // float maxSpeedChange = acceleration * Time.deltaTime;
        // velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        // velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);

        if (desiredJump)
        {
            desiredJump = false;
            Jump();
        }

        body.velocity = velocity;

        ClearState();
    }

    private void ClearState()
    {
        groundContactCount = 0;
        contactNormal = Vector3.zero;
    }

    private void UpdateState()
    {
        velocity = body.velocity;
        if (OnGround)
        {
            jumpPhase = 0;
            if (groundContactCount > 1)
                contactNormal.Normalize();
        }
        else
            contactNormal = Vector3.up; // To facilitate air jumps after jumping from slope
    }

    // Since our movement in XZ plane is always driven by velocity in same plane, we also need to apply the same to the velocity on slope
    void AdjustVelocity()
    {
        Vector3 xAxis = ProjectOnContactPlane(transform.right).normalized;
        Vector3 zAxis = ProjectOnContactPlane(transform.forward).normalized;

        float currentX = Vector3.Dot(velocity, xAxis);
        float currentZ = Vector3.Dot(velocity, zAxis);
        // this will give x and z values wrt to plane, which will be less than xz planar velocity if going upwards else vice versa

        // Sideways
        float acceleration = OnGround ? maxSideAcceleration : maxSideAirAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;
        float newX = Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);

        // Forward
        acceleration = OnGround ? maxFwdAcceleration : maxFwdAirAcceleration;
        maxSpeedChange = acceleration * Time.deltaTime;
        float newZ = Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

        // we can directly use newX and newZ, since we currentX and currentZ are the actual velocity and not incrementing velocity
        // we need incrementing velocity so we can add to current velocity
        // this protocol of adding to current is necessary, if the objects is going to be controlled by various other scripts 
        // else a directly assigning solution will just neglect other script calculations by resetting in next Update
        velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }

    private float GetRunMultiplier()
    {
        if (!desiredRun) return 1;
        return Mathf.Lerp(1, runSpeedMultiplier, runMultiplierStrength);
    }

    // This method gives Vector on the plan, Vector3.ProjectOnPlane can also be used but
    Vector3 ProjectOnContactPlane(Vector3 vector)
    {
        return vector - contactNormal * Vector3.Dot(vector, contactNormal);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // This solution is not ideal if there are too many collision objects in environment
        // since it will set the bool and will be impossible to jump again
        // onGround = true;
        EvaluateCollision(collision);
    }

    // If multiple collision happen on the object we set onGround depending on contact normals
    // since we are using sphere every contact normal will point to the center of it
    // therefore we check if normal.y is > 0.9, since its float value
    private void EvaluateCollision(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            // onGround |= normal.y >= minGroundDotProduct; // this just facilitates the readiness of jump, but we need to jump as per slope normal

            if (normal.y >= minGroundDotProduct)
            {
                if (groundContactCount == 0)
                    OnAground?.Invoke();
                groundContactCount++;
                // This only prefers last contact point and is not ideal since we cant know how PhysX evaluates the order of contacts
                // but despite knowing the working og PhysX engine the best approach would be to get average normal of all the contact points
                // contactNormal = normal;
                contactNormal += normal;
            }
        }
    }

    // Instead we set the bool for the lifetime of collisions and unset in next FixedUpdate, i.e. at the start of next PhysX time step
    // private void OnCollisionExit(Collision collision)
    private void OnCollisionStay(Collision collision)
    {
        // onGround = true;
        EvaluateCollision(collision);
    }

    private void Jump()
    {
        OnJump?.Invoke();
        if (OnGround || jumpPhase++ < maxAirJumps)
        {
            // We start with an initial jump velocity j, which gets reduced by gravity until it reaches zero, after which we start to fall back down. 
            // Gravity 'g' is a constant acceleration that pulls us down,
            // for which we use a positive number in this derivation as that saves us from writing a bunch of minuses. 
            // So at any time 't' since jumping the vertical velocity is  v = j − gt .
            // When 'v'  reaches zero we're at the top of the jump, so exactly at the desired height.
            // This happens when  j − gt = 0 , so when  j = gt .
            // Thus the top of the jump is reached when  t = jg .
            // Because 'g' is constant the average speed at any time is  vₐᵥ = (j − gt)/2 ,
            // thus the height at any time is  h = vₐᵥ.t = jt − gt²/2.
            // This means that at the top of the jump  h = j(j/g) − ( g(j/g)² / 2 ),
            // which we can rewrite to h = j²g − j²g2 = j²g − j²2g = j²2g
            // Now we know that h=j²2g  at the top,
            // thus j² = 2gh and j = √2gh.
            // When 'g' is a negative number instead then 
            // j = √−2gh.

            float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);     // jump speed at any point is √(-2gh)
            float alignedSpeed = Vector3.Dot(velocity, contactNormal);
            if (alignedSpeed > 0)
                jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0);
            // velocity.y += jumpSpeed; // we need to jump as per the slope normal
            velocity += contactNormal * jumpSpeed;
        }
    }
}
