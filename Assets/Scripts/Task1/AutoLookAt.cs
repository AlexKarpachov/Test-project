using UnityEngine;

/// <summary>
/// Automatically rotates a GameObject towards a target Transform over time.
/// Ignores Y-axis for horizontal-only rotation (e.g., for ground-based aiming).
/// Uses smooth interpolation (RotateTowards) for natural turning speed.
/// </summary>
public class AutoLookAt : MonoBehaviour
{
    [SerializeField] Transform target;        // The target to look at (e.g., player or enemy)
    [SerializeField] float rotationSpeed = 36f;  // Degrees per second for smooth rotation (higher = faster turn)

    private void Update()
    {
        if (target == null) return;

        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude < 0.0001f) return;  

        Quaternion rotation = Quaternion.LookRotation(toTarget.normalized, Vector3.up);

        // Smoothly interpolate current rotation towards target (max angle = speed * deltaTime)
        transform.rotation = Quaternion.RotateTowards
        (
            transform.rotation,  
            rotation,            
            rotationSpeed * Time.deltaTime  
        );
    }
}