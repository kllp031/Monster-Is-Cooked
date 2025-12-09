using System.Runtime.CompilerServices;
using TMPro;
using UnityEditor;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private Vector2 topLeft, bottomRight;
    [SerializeField] private MapArea homeArea;
    private float heightCamera, widthCamera;

    private void Start()
    {
        //get the height and width of the camera
        heightCamera = 2f * Camera.main.orthographicSize;
        widthCamera = heightCamera * Camera.main.aspect;

        BackHome();
    }
    void Update()
    {
        //transform.position = new Vector3(playerTransform.position.x, playerTransform.position.y, transform.position.z);

        // Smoothly follow the player
        Vector3 targetPosition = new Vector3( playerTransform.position.x, playerTransform.position.y, transform.position.z);

        // Clamp the camera position within the defined bounds
        float clampedX = Mathf.Clamp(targetPosition.x, topLeft.x + widthCamera / 2f, bottomRight.x - widthCamera / 2f);
        float clampedY = Mathf.Clamp(targetPosition.y, bottomRight.y + heightCamera / 2f,    topLeft.y - heightCamera / 2f);
        targetPosition = new Vector3(clampedX, clampedY, targetPosition.z);

        transform.position = Vector3.Lerp( transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }

    public void SetBoundaries(Vector2 topLeft, Vector2 bottomRight)
    {
        this.bottomRight = bottomRight;
        this.topLeft = topLeft;
    }

    public void BackHome()
    {
        SetBoundaries(
                homeArea.getTopLeft(),
                homeArea.getBottomRight()
            );
    }
}
