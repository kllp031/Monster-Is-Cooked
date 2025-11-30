using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class KnightAttack : MonoBehaviour
{
    [SerializeField] private GameObject attackEffect;
    [SerializeField] private float coolDownTime = 0.5f;
    [SerializeField] private float lockAttackRange = 10.0f; //Player will lock on to the nearest enemy within this range, else attack forward
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float distanceAttack = 1.0f; // Distance from player to spawn attack effect
    private Vector2 attackDirection = Vector2.down;
    private Coroutine coolDownCoroutine = null;
    private KnightController knightController;

    private void Awake()
    {
        knightController = GetComponent<KnightController>();
        if (knightController == null)
        {
            Debug.LogError("KnightController null");
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started && coolDownCoroutine == null)
        {
            if (attackEffect != null)
            {
                // Determine attack direction
                Transform enemyTransform = FindNearestEnemy();
                if(enemyTransform == null)
                    attackDirection = knightController.getMoveInput();
                else
                    attackDirection = (enemyTransform.position - transform.position).normalized;

                // Set position and rotation of attack effect
                attackEffect.transform.position = transform.position + new Vector3(attackDirection.x, attackDirection.y, 0).normalized * distanceAttack;
                attackEffect.transform.up = -attackDirection;

                // Flip the attack 
                Vector3 scale = attackEffect.transform.localScale;
                scale.x = -scale.x;
                attackEffect.transform.localScale = scale;

                // Attack
                attackEffect.SetActive(true);
                attackEffect.GetComponent<Animator>().SetTrigger("Attack");
                coolDownCoroutine = StartCoroutine(CoolDownCoroutine());
            }
        }
    }
    IEnumerator CoolDownCoroutine()
    {
        yield return new WaitForSeconds(coolDownTime);
        coolDownCoroutine = null;
    }

    Transform FindNearestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, lockAttackRange, enemyLayer);
        Transform closest = null;
        float minDist = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            Debug.Log("hit");
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = hit.transform;
            }
        }

        return closest;
    }

    // Draw to debug
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lockAttackRange);
    }
}
