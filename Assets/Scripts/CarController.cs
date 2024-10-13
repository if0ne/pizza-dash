using Mirror;
using System.Collections;
using UnityEngine;

public class CarController : NetworkBehaviour
{
    public Joystick joystick; // Reference to the joystick

    public float maxSpeed = 20f; // Maximum speed of the car
    public float acceleration = 5f; // Acceleration rate
    public float rotationSpeed = 100f; // Rotation speed of the car
    private float currentSpeed = 0f; // Current speed of the car

    private Rigidbody rb;
    private Coroutine slowDownCoroutine; // Reference to the current coroutine
    public float slowDownMul = 0.5f;
    public float slowDownTime = 3.0f;

    public float originalMaxSpeed; // Store original max speed
    public float originalAcceleration; // Store original acceleration

    // Ensure that only the local player controls their car
    public override void OnStartLocalPlayer()
    {
        // Assign the joystick only for the local player
        joystick = FindObjectOfType<Joystick>();

        // Disable the camera for non-local players (optional, if you're using individual cameras)
        Camera.main.GetComponent<CameraFollow>().player = transform;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        originalMaxSpeed = maxSpeed; // Initialize original max speed
        originalAcceleration = acceleration; // Initialize original acceleration
    }

    void FixedUpdate()
    {
        if (!isLocalPlayer) return; // Only allow local player to control the car

        float moveVertical = joystick.Vertical;
        float moveHorizontal = joystick.Horizontal;

        // Calculate the direction based on joystick input
        Vector3 direction = new Vector3(moveHorizontal, 0, moveVertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            // Calculate the target angle and rotate the car
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationSpeed, 0.1f);
            transform.rotation = Quaternion.Euler(0, angle, 0);

            // Accelerate the car
            currentSpeed += acceleration * Time.deltaTime;
            currentSpeed = Mathf.Clamp(currentSpeed, 0, maxSpeed);
        }
        else
        {
            // Decelerate the car when joystick is not being used
            currentSpeed = Mathf.Lerp(currentSpeed, 0, Time.deltaTime * acceleration * 0.05f);
        }

        // Move the car in the direction
        Vector3 move = transform.forward * currentSpeed;
        rb.velocity = move;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Ground") && currentSpeed >= maxSpeed * 0.9f) // Check if the car is at or near maximum speed
        {
            rb.velocity = Vector3.zero;
            currentSpeed = 0;

            if (slowDownCoroutine != null)
            {
                StopCoroutine(slowDownCoroutine); // Stop the current coroutine if it exists
                ResetSpeedAndAcceleration(); // Reset speed and acceleration to original values
            }

            slowDownCoroutine = StartCoroutine(SlowDownAfterCrash());
        }
    }

    private IEnumerator SlowDownAfterCrash()
    {
        float originalMaxSpeed = maxSpeed;
        float originalAcceleration = acceleration;

        maxSpeed *= slowDownMul; // Reduce max speed by half
        acceleration *= slowDownMul; // Reduce acceleration by half

        yield return new WaitForSeconds(slowDownTime);

        maxSpeed = originalMaxSpeed; // Restore original max speed
        acceleration = originalAcceleration; // Restore original acceleration
    }

    private void ResetSpeedAndAcceleration()
    {
        maxSpeed = originalMaxSpeed; // Restore original max speed
        acceleration = originalAcceleration; // Restore original acceleration
    }
}
