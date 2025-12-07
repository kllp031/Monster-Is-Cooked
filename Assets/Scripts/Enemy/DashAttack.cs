using System.Collections;
using UnityEngine;

public class DashAttack : EnemyAttackBase
{
    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 15f;     
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private GameObject damageAreaPrefab;

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (damageAreaPrefab != null) damageAreaPrefab.SetActive(false);
    }

    public override void PerformAttack()
    {
        if (!CanAttack()) return;

        Vector2 direction = Vector2.zero;

        var enemyScript = GetComponent<EnemyBase>();
        if (enemyScript != null && enemyScript.target != null)
        {
            direction = (enemyScript.target.position - transform.position).normalized;
        }
        else
        {
            direction = rb.linearVelocity.normalized;
        }

        Debug.Log("dash direction x:" + direction.x + " y: " + direction.y);
        StartCoroutine(DashRoutine(direction));
        ResetCooldown();
    }

    private IEnumerator DashRoutine(Vector2 direction)
    {
        if (damageAreaPrefab != null) damageAreaPrefab.SetActive(true); 

        rb.linearVelocity = direction * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        rb.linearVelocity = Vector2.zero; 
        if (damageAreaPrefab != null) damageAreaPrefab.SetActive(false); 
    }
}
