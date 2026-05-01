using Fusion;
using System.Collections;
using UnityEngine;

/// <summary>
/// Online version of <see cref="KnightAttack"/>.
///
/// Khác với bản single-player:
/// - Inherit <see cref="NetworkBehaviour"/> để dùng được RPC.
/// - Trigger qua <see cref="OnButtonAttack"/> (gọi từ UI button qua
///   <c>PlayerActionsProxy.Attack</c>), không đọc <see cref="UnityEngine.InputSystem.InputAction.CallbackContext"/>
///   trực tiếp — đồng bộ với pipeline input hiện tại.
/// - VFX/animator được phát trên TẤT CẢ clients qua <see cref="RPC_PlayAttack"/>
///   để player khác nhìn thấy đòn đánh, không chỉ chạy local.
/// - Damage vẫn dựa trên collider của <c>attackEffect</c> + <c>Damage.cs</c>
///   (chưa networked). Hệ <see cref="Health"/> còn là <c>MonoBehaviour</c>
///   nên mỗi client simulate HP độc lập — cần rework Health → NetworkBehaviour
///   để damage authoritative.
/// </summary>
[RequireComponent(typeof(KnightControllerOnline))]
public class KnightAttackOnline : NetworkBehaviour
{
    [SerializeField] private GameObject attackEffect;
    [SerializeField] private float coolDownTime = 0.5f;
    [SerializeField] private float lockAttackRange = 10.0f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float distanceAttack = 0.5f;

    private Coroutine coolDownCoroutine = null;
    private KnightControllerOnline knightController;
    private Animator animator;
    private Health health;

    private void Awake()
    {
        knightController = GetComponent<KnightControllerOnline>();
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();
    }

    /// <summary>
    /// Gọi từ UI button (qua <c>PlayerActionsProxy.Attack</c>). Chỉ client của
    /// player này (<see cref="NetworkObject.HasInputAuthority"/>) trigger; sau
    /// đó RPC broadcast cho mọi client play VFX + animator.
    /// </summary>
    public void OnButtonAttack()
    {
        if (Object == null || !Object.HasInputAuthority) return;
        if (health != null && health.isDeath) return;
        if (animator != null && animator.GetCurrentAnimatorStateInfo(0).IsTag("Hurt")) return;
        if (coolDownCoroutine != null) return;
        if (attackEffect == null) return;

        Vector2 dir = ComputeAttackDir();
        RPC_PlayAttack(dir);
        coolDownCoroutine = StartCoroutine(CoolDownCoroutine());
    }

    private Vector2 ComputeAttackDir()
    {
        Transform enemy = FindNearestEnemy();
        if (enemy != null)
            return ((Vector2)(enemy.position - transform.position)).normalized;
        return knightController != null ? knightController.GetMoveInput() : Vector2.zero;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_PlayAttack(Vector2 dir)
    {
        if (attackEffect == null) return;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(SoundManager.Instance.playerAttack);

        Vector3 dir3 = new Vector3(dir.x, dir.y, 0).normalized;
        attackEffect.transform.position = transform.position + dir3 * distanceAttack;
        attackEffect.transform.up = -dir3;
        if (dir == Vector2.zero)
            attackEffect.transform.position = transform.position + new Vector3(0, -1f, 0);

        // Mirror đúng hành vi single-player: flip localScale.x mỗi attack.
        Vector3 scale = attackEffect.transform.localScale;
        scale.x = -scale.x;
        attackEffect.transform.localScale = scale;

        if (animator != null) animator.SetTrigger("Attack");
        attackEffect.SetActive(true);
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
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = hit.transform;
            }
        }
        return closest;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lockAttackRange);
    }
}
