using UnityEngine;
using UnityEngine.Events;
using Fusion;

public class Gate : MonoBehaviour
{
    [SerializeField] private MapArea targetArea;
    [SerializeField] UnityEvent onTeleport = new();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        var networkObject = other.GetComponent<NetworkObject>();
        if (networkObject == null)
            return;

        if (!networkObject.HasStateAuthority)
            return;

        // 1. Move player to spawn point of target area.
        //    Dùng NetworkTransform.Teleport để Fusion cập nhật state buffer,
        //    tránh việc resimulation đè vị trí mới trở lại vị trí cũ.
        Transform player = other.transform;
        Vector3 targetPos = targetArea.getSpawnPosition();

        var netTransform = other.GetComponent<NetworkTransform>();
        if (netTransform != null)
        {
            netTransform.Teleport(targetPos);
        }
        else
        {
            player.position = targetPos;
        }
        //Debug.Log("tele to" + targetPos);

        // 2. Get Main Camera and update its boundaries
        CameraFollow cam = Camera.main.GetComponent<CameraFollow>();
        if (cam != null)
        {
            cam.SetBoundaries(
                targetArea.getTopLeft(),
                targetArea.getBottomRight()
            );
            cam.transform.position = new Vector3(
                player.position.x,
                player.position.y,
                cam.transform.position.z
            );
        }

        onTeleport.Invoke();
    }
}
