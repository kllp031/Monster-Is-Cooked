using System.Runtime.CompilerServices;
using TMPro;
using UnityEditor;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private Vector2 topLeft, bottomRight;
    private float height, width;

    private void Start()
    {
        //get the height and width of the camera
        height = 2f * Camera.main.orthographicSize;
        width = height * Camera.main.aspect;
    }
    void Update()
    {
        //transform.position = new Vector3(playerTransform.position.x, playerTransform.position.y, transform.position.z);

        // Smoothly follow the player
        Vector3 targetPosition = new Vector3( playerTransform.position.x, playerTransform.position.y, transform.position.z);

        // Clamp the camera position within the defined bounds
        float clampedX = Mathf.Clamp(targetPosition.x, topLeft.x, bottomRight.x);
        float clampedY = Mathf.Clamp(targetPosition.y, bottomRight.y, topLeft.y);
        targetPosition = new Vector3(clampedX, clampedY, targetPosition.z);

        transform.position = Vector3.Lerp( transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }

    public void SetBoundaries(Vector2 topLeft, Vector2 bottomRight)
    {
        this.bottomRight = bottomRight;
        this.topLeft = topLeft;
    }
}
