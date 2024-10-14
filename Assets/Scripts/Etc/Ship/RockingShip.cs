using UnityEngine;

//배가 물위에 떠다니는 효과를 주기 위한 스크립트
public class RockingShip : MonoBehaviour
{
    public float amplitude = 0.05f; // How high the object bobs
    public float frequency = 1f; // How fast the object bobs

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position; // Store the starting position
    }

    void Update()
    {
        // Calculate the new Y position
        float newY = startPosition.y + Mathf.Sin(Time.time * frequency) * amplitude;
        
        // Apply the new position
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
}