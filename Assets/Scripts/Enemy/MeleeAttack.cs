using UnityEngine;
using System.Collections;

public class MeleeAttack : EnemyAttackBase
{
    [Header("Melee Specifics")]
    [SerializeField] private GameObject _damageObject;
    [SerializeField] private float _hitboxActiveTime = 0.2f; 
    [SerializeField] private float _startDelay = 0.1f;

    private void Start()
    {
        if (_damageObject != null) _damageObject.SetActive(false);
    }

    public override void PerformAttack()
    {
        if (!CanAttack()) return;

        StartCoroutine(AttackRoutine());
        ResetCooldown();
    }

    private IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(_startDelay);

        _damageObject.SetActive(true);

        yield return new WaitForSeconds(_hitboxActiveTime);

        _damageObject.SetActive(false);
    }
}