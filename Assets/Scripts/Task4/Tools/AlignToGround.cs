using UnityEngine;

/// <summary>
/// Component to align a GameObject to the ground using raycasting.
/// Performs a downward raycast to detect the surface, positions the object on it,
/// and optionally rotates it to match the surface normal (slope alignment).
/// Supports self-collider filtering to avoid hitting the object itself.
/// Use in Editor via custom button or context menu; callable in runtime.
/// </summary>
public class AlignToGround : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] float maxRayDistance = 200f;  // Maximum distance for the downward raycast (e.g., for tall terrain)
    [SerializeField] LayerMask groundMask = ~0;    // Layer mask to filter what counts as "ground" (default: all layers)
    [SerializeField] float surfaceOffset = 0.0f;   // Offset from the hit point along the normal (positive: above surface, negative: below)

    [Header("Rotation")]
    [SerializeField] bool alignToSlope = true;     // If true, rotates the object to align its up-axis with the surface normal
    [SerializeField] bool preserveYaw = true;      // If true (and alignToSlope=true), preserves the object's forward direction (projects onto plane)

    /// <summary>
    /// Attempts to align the object to the ground using a downward raycast.
    /// Filters out hits from the object itself or its children.
    /// Positions the object at the hit point + offset, and optionally rotates it.
    /// </summary>
    /// <param name="hit">Out: The closest valid RaycastHit (surface point, normal, etc.). Default if no hit.</param>
    /// <returns>True if a valid ground hit was found and alignment succeeded; false otherwise.</returns>
    public bool TryAlign(out RaycastHit hit)
    {
        hit = new RaycastHit();

        Vector3 origin = transform.position + Vector3.up * (maxRayDistance * 0.5f);
        Ray ray = new Ray(origin, Vector3.down);  // Ray direction: straight down

        // Perform raycast on all colliders within distance, ignoring triggers
        // Returns all hits along the ray (not just the closest, to allow filtering)
        RaycastHit[] hits = Physics.RaycastAll(ray, maxRayDistance, groundMask, QueryTriggerInteraction.Ignore);

        // Sort hits by distance (closest first) for proper nearest-surface detection
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        // Iterate through sorted hits to find the first valid (non-self) ground hit
        foreach (RaycastHit h in hits)
        {
            // Skip hits from this object or its children (avoids self-collision, e.g., object's own collider)
            if (h.collider != null && (h.collider.transform == transform || h.collider.transform.IsChildOf(transform)))
                continue;

            // Valid hit found: Use this one
            hit = h;  

            // Position the object at the hit point, offset along the surface normal
            // (e.g., surfaceOffset=0 places pivot at surface; >0 lifts it up)
            transform.position = hit.point + hit.normal * surfaceOffset;

            if (alignToSlope)
            {
                if (preserveYaw)
                {
                    Vector3 fwd = Vector3.ProjectOnPlane(transform.forward, hit.normal);
                    // Fallback if projection is too small (degenerate case, e.g., steep slope)
                    if (fwd.sqrMagnitude < 0.0001f)
                        fwd = Vector3.Cross(hit.normal, transform.right);  
                    transform.rotation = Quaternion.LookRotation(fwd.normalized, hit.normal);
                }
                else
                {
                    // Simple alignment: Rotate only to match up-axis to surface normal (may change yaw)
                    transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
                }
            }
            return true; 
        }

        return false;
    }

    /// <summary>
    /// Context menu item for quick alignment in the Editor.
    /// Right-click the GameObject in Hierarchy > "Align To Ground (Now)".
    /// Only executes in Editor (via #if UNITY_EDITOR).
    /// Logs a warning if alignment fails.
    /// </summary>
    [ContextMenu("Align To Ground (Now)")]
    private void ContextAlignNow()
    {
#if UNITY_EDITOR
        RaycastHit _;  
        if (!TryAlign(out _))
            Debug.LogWarning("AlignToGround: nothing under object.", this);
#endif
    }
}
