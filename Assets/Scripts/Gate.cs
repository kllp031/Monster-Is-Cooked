using UnityEngine;

public class Gate : MonoBehaviour
{
    [SerializeField] private MapArea targetArea;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        // 1. Move player to spawn point of target area
        Transform player = other.transform;
        player.position = targetArea.getSpawnPosition();

        // 2. Get Main Camera and update its boundaries
        CameraFollow cam = Camera.main.GetComponent<CameraFollow>();
        if (cam != null)
        {
            cam.SetBoundaries(
                targetArea.getTopLeft(),
                targetArea.getBottomRight()
            );
        }
    }
}
