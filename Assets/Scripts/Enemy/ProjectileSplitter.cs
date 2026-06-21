using UnityEngine;
using Fusion;

public class ProjectileSplitter : MonoBehaviour
{
    [SerializeField] private GameObject _fragmentPrefab;
    [SerializeField] private int _fragmentCount = 6;

    public void Split(NetworkRunner runner, NetworkObject sourceObject)
    {
        if (_fragmentPrefab == null) return;

        float angleStep = 360f / _fragmentCount;
        float startAngle = Random.Range(0f, 360f);

        for (int i = 0; i < _fragmentCount; i++)
        {
            float currentAngle = startAngle + i * angleStep;
            Quaternion rotation = Quaternion.Euler(0, 0, currentAngle);
            Vector2 direction = rotation * Vector2.right;

            NetworkObject fragment = runner.Spawn(_fragmentPrefab, transform.position, rotation, sourceObject.InputAuthority);
            if (fragment != null && fragment.TryGetComponent(out BulletMove bullet))
                bullet.SetDirection(direction);
        }
    }
}
