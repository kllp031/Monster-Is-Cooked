using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion; 

public class Damage : NetworkBehaviour 
{
    [Header("Team Settings")]
    [Tooltip("The team associated with this damage")]
    public int teamId = 0;

    [Header("Damage Settings")]
    [Tooltip("How much damage to deal")]
    public int damageAmount = 1;
    public int knockbackForce = 50;
    [Tooltip("Whether or not to destroy the attached game object after dealing damage")]
    public bool destroyAfterDamage = true;
    [Tooltip("Whether or not to apply damage when triggers collide")]
    public bool dealDamageOnTriggerEnter = false;
    [Tooltip("Whether or not to apply damage when triggers stay, for damage over time")]
    public bool dealDamageOnTriggerStay = false;
    [Tooltip("Whether or not to apply damage on non-trigger collider collisions")]
    public bool dealDamageOnCollision = false;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Object != null && !Object.HasStateAuthority) return;

        if (dealDamageOnTriggerEnter)
        {
            DealDamage(collision.gameObject);
            DealKnockback(collision.gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (Object != null && !Object.HasStateAuthority) return;

        if (dealDamageOnTriggerStay)
        {
            DealDamage(collision.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (Object != null && !Object.HasStateAuthority) return;

        if (dealDamageOnCollision)
        {
            DealDamage(collision.gameObject);
        }
    }

    private void DealDamage(GameObject collisionGameObject)
    {
        Health collidedHealth = collisionGameObject.GetComponent<Health>();
        if (collidedHealth != null)
        {
            if (collidedHealth.teamId != this.teamId)
            {
                Debug.Log("take damage via network");

                if (this.gameObject.CompareTag("PlayerAttack"))
                {
                    if (SoundManager.Instance != null)
                        SoundManager.Instance.PlaySFX(SoundManager.Instance.metalHit);

                    // Master Client gọi trừ máu, hàm này bên trong đã có sẵn cấu trúc RPC kết nối sang Health mạng
                    if (PlayerDataManager.Instance != null)
                        collidedHealth.TakeDamage(PlayerDataManager.Instance.CurrentAttack);
                }
                else
                {
                    collidedHealth.TakeDamage(damageAmount);
                }

                if (destroyAfterDamage)
                {
                    Debug.Log("despawn network bullet/vfx");
                    // 2. Thay thế Destroy thường bằng Despawn mạng chuẩn của Fusion
                    Runner.Despawn(Object);
                }
            }
        }
    }

    private void DealKnockback(GameObject collisionGameObject)
    {
        var dir = (collisionGameObject.transform.position - transform.position).normalized;
        Health collidedHealth = collisionGameObject.GetComponent<Health>();
        if (collidedHealth != null)
        {
            if (collidedHealth.teamId != this.teamId)
            {
                // Xử lý giật khựng trạng thái cho Player cục bộ qua mạng
                if (collisionGameObject.CompareTag("Player"))
                {
                    var knightController = collisionGameObject.GetComponent<KnightController>();
                    if (knightController != null)
                    {
                        knightController.SetIsHurting(true);
                    }
                }
                collidedHealth.Knockback(dir, knockbackForce);
            }
        }
    }
}