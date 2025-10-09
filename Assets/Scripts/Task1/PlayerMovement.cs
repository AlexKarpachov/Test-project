using UnityEngine;

/// <summary>
/// Handles horizontal/vertical input (WASD/arrows) for world-space translation.
/// Normalizes diagonal movement to prevent faster speed on diagonals.
/// Attach to a player GameObject; adjust moveSpeed in Inspector for tuning.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;  

    private void Update()
    {
        Movement();  
    }

    private void Movement()
    {
        // Get input axes: Horizontal (left/right, -1 to 1), Vertical (forward/back, -1 to 1)
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Create movement vector (Y=0 for flat ground movement, no up/down)
        Vector3 move = new Vector3(x, 0f, z);

        // Normalize if diagonal input (sqrt(2) >1) to cap speed at moveSpeed
        if (move.sqrMagnitude > 1f)
            move.Normalize();

        // Translate in world space (ignores local rotation; always forward=Z, right=X)
        // Multiplies by speed and deltaTime for smooth, consistent movement
        transform.Translate(move * moveSpeed * Time.deltaTime, Space.World);
    }
}