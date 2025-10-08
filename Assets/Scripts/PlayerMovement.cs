using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private void Update()
    {
        Movement();
    }

    private void Movement()
    {
        float x = Input.GetAxis("Horizontal"); 
        float z = Input.GetAxis("Vertical");   

        Vector3 move = new Vector3(x, 0f, z);

        if (move.sqrMagnitude > 1f)
            move.Normalize();

        transform.Translate(move * moveSpeed * Time.deltaTime, Space.World);
    }
}
