using UnityEngine;

public class ProjectileSplitter : MonoBehaviour
{
    [SerializeField] private GameObject _fragmentPrefab; 
    [SerializeField] private int _fragmentCount = 6;       

    private bool _isQuitting = false;

    private void OnApplicationQuit()
    {
        _isQuitting = true;
    }

    private void OnDestroy()
    {
        Debug.Log("on des bullet");
        if (_isQuitting || !gameObject.scene.isLoaded) return;

        SpawnFragments();
    }

    private void SpawnFragments()
    {
        if (_fragmentPrefab == null) return;

        Debug.Log("spawn fragment");
        float angleStep = 360f / _fragmentCount;
        float startAngle = Random.Range(0f, 360f); 

        for (int i = 0; i < _fragmentCount; i++)
        {
            float currentAngle = startAngle + (i * angleStep);

            //Vector2 direction = Quaternion.Euler(0, 0, currentAngle) * Vector2.right;

            Quaternion rotation = Quaternion.Euler(0, 0, currentAngle);
            var fragment = Instantiate(_fragmentPrefab, transform.position, rotation);
            fragment.GetComponent<BulletMove>().SetDirection(Vector2.right);
        }
    }
}
