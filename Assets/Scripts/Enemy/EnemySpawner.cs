using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private int _maxEnemies = 10;       
    [SerializeField] private float _spawnInterval = 2f; 

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
        Debug.Log("var");
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
            // ClearEnemies(); 
        }
    }

    private IEnumerator SpawnRoutine()
    {
        while (_isActive)
        {
            if (_currentCount < _maxEnemies)
            {
                SpawnEnemy();
            }
            yield return new WaitForSeconds(_spawnInterval);
        }
    }

    private void SpawnEnemy()
    {
        Vector2 spawnPos = GetRandomPositionInBounds();

        GameObject newEnemy = Instantiate(_enemyPrefab, spawnPos, Quaternion.identity);
        newEnemy.GetComponent<EnemyBase>().SetTarget(player);
        if (newEnemy.TryGetComponent(out Health healthScript))
        {
            healthScript.SetupSpawner(this);
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

    public void OnEnemyDeath()
    {
        _currentCount--;
        if (_currentCount < 0) _currentCount = 0;
    }
}
