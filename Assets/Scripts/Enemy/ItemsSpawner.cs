using System.Collections;
using UnityEngine;

public class ItemsSpawner : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject _itemPrefab; 
    [SerializeField] private int _maxItems = 5;      
    [SerializeField] private float _spawnInterval = 3f; 

    [Header("Debug Info")]
    [SerializeField] private int _currentCount = 0;
    [SerializeField] private bool _isActive = false;

    private BoxCollider2D _spawnArea;
    private Coroutine _spawnCoroutine;

    private void Awake()
    {
        _spawnArea = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isActive = true;
            if (_spawnCoroutine == null)
                _spawnCoroutine = StartCoroutine(SpawnRoutine());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isActive = false;
            if (_spawnCoroutine != null)
            {
                StopCoroutine(_spawnCoroutine);
                _spawnCoroutine = null;
            }
        }
    }

    private IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        while (_isActive)
        {
            if (_currentCount < _maxItems)
            {
                SpawnItem();
            }
            yield return new WaitForSeconds(_spawnInterval);
        }
    }

    private void SpawnItem()
    {
        Vector2 spawnPos = GetRandomPositionInBounds();

        GameObject newItem = Instantiate(_itemPrefab, spawnPos, Quaternion.identity);

        if (newItem.TryGetComponent(out PickupItem itemScript))
        {
            //itemScript.SetupSpawner(this);
            _currentCount++;
        }
    }

    private Vector2 GetRandomPositionInBounds()
    {
        Bounds bounds = _spawnArea.bounds;
        float x = Random.Range(bounds.min.x, bounds.max.x);
        float y = Random.Range(bounds.min.y, bounds.max.y);
        return new Vector2(x, y);
    }

    public void OnItemCollected()
    {
        _currentCount--;
        if (_currentCount < 0) _currentCount = 0;
    }
}
