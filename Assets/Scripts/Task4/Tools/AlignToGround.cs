using UnityEngine;

public class AlignToGround : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] float maxRayDistance = 200f;
    [SerializeField] LayerMask groundMask = ~0;
    [SerializeField] float surfaceOffset = 0.0f;

    [Header("Rotation")]
    [SerializeField] bool alignToSlope = true;
    [SerializeField] bool preserveYaw = true;

    public bool TryAlign(out RaycastHit hit)
    {
        hit = new RaycastHit(); 

        Vector3 origin = transform.position + Vector3.up * (maxRayDistance * 0.5f);
        Ray ray = new Ray(origin, Vector3.down);

        RaycastHit[] hits = Physics.RaycastAll(ray, maxRayDistance, groundMask, QueryTriggerInteraction.Ignore);

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit h in hits)
        {
            if (h.collider != null && (h.collider.transform == transform || h.collider.transform.IsChildOf(transform)))
                continue;

            hit = h;
            transform.position = hit.point + hit.normal * surfaceOffset;

            if (alignToSlope)
            {
                if (preserveYaw)
                {
                    Vector3 fwd = Vector3.ProjectOnPlane(transform.forward, hit.normal);
                    if (fwd.sqrMagnitude < 0.0001f)
                        fwd = Vector3.Cross(hit.normal, transform.right);
                    transform.rotation = Quaternion.LookRotation(fwd.normalized, hit.normal);
                }
                else
                {
                    transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
                }
            }
            return true;
        }

        return false;
    }

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
