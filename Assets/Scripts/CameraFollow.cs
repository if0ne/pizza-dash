using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // Reference to the player's transform
    public Joystick joystick; // Reference to the joystick
    public float followSpeed = 5f; // Speed at which the camera follows the player
    public float deflectionAmount = 2f; // Amount of deflection when the player moves

    public Vector3 originalOffset; // Original offset from the player

    void FixedUpdate()
    {
        if (player == null) { return; }

        // Get input from the joystick
        float moveVertical = joystick.Vertical;
        float moveHorizontal = joystick.Horizontal;

        // Calculate the deflection based on joystick input
        Vector3 deflection = new Vector3(moveHorizontal, 0, moveVertical) * deflectionAmount;

        if (deflection.normalized.magnitude >= 0.1) {
            // Calculate the target position with deflection
            Vector3 targetPosition = player.position + originalOffset + deflection;

            // Smoothly move the camera towards the target position
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        }
        else 
        {
            // If the joystick is not being used, return to the original offset
            transform.position = Vector3.Lerp(transform.position, player.position + originalOffset, followSpeed * Time.deltaTime);
        }
    }
}
