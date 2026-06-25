using UnityEngine;

public class WaterMovement : MonoBehaviour
{
    void Update()
    {
        transform.position = new Vector3(
            transform.position.x,
            Mathf.Sin(Time.time) * 0.1f,
            transform.position.z
        );
    }
}