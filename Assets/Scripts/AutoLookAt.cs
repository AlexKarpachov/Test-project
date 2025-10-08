using UnityEngine;

public class AutoLookAt : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float rotationSpeed = 36f;

    private void Update()
    {
        if (target == null) return;

        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude < 0.0001f) return; // уже націлені (дуже близько)

        Quaternion rotation = Quaternion.LookRotation(toTarget.normalized, Vector3.up);

        transform.rotation = Quaternion.RotateTowards
        (
            transform.rotation,
            rotation,
            rotationSpeed * Time.deltaTime
        );
    }
}
