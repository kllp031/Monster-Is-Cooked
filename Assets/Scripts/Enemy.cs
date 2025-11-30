using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{
    public GameObject itemToDrop;

    [Header("Settings")]
    public float moveSpeed = 3f;
    public float attackRange = 0.5f;
    public float attackCooldown = 1f;
    public float attackDelay = 1f;

    private UnityEvent onDeath = new UnityEvent();
    private float lastAttackTime = 0f;
    private GameObject player;

    private void OnEnable()
    {
        onDeath.AddListener(DropItem);
    }

    private void OnDisable()
    {
        onDeath.RemoveListener(DropItem);
    }

    private void OnValidate()
    {
        // Only warn for scene instances (not prefab assets)
        if (!gameObject.scene.IsValid())
            return;

        if (itemToDrop == null)
        {
            Debug.LogWarning($"{nameof(Enemy)} on '{name}' has no {nameof(itemToDrop)} assigned.", this);
        }
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        Chase();
    }

    private void Chase()
    {
        if (player == null) return;

        // khoang cach tu enemy den player
        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance > attackRange)
        {
            // duoi theo player neu ngoai pham vi tan cong
            Vector3 dir = (player.transform.position - transform.position).normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;
        }
        else
        {
            // tan cong neu player trong pham vi
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                StartCoroutine(Attack());
                lastAttackTime = Time.time;
            }
        }
    }

    private IEnumerator Attack()
    {
        yield return new WaitForSeconds(attackDelay);
        //Debug.Log($"{name} attacks {player.name}!");
    }

    public void Die()
    {
        DropItem();
        Destroy(gameObject);
    }

    private void DropItem()
    {
        if (itemToDrop == null)
        {
            Debug.LogError("Enemy.itemToDrop is not assigned.", this);
            return;
        }

        Instantiate(itemToDrop, transform.position, Quaternion.identity);
    }
}
