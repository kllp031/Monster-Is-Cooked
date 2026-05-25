using UnityEngine;
using Fusion; 

public class MeleeAttack : EnemyAttackBase 
{
    [Header("Melee Specifics")]
    [SerializeField] private GameObject _damageObject;
    [SerializeField] private float _hitboxActiveTime = 0.2f;
    [SerializeField] private float _startDelay = 0.1f;

    [Networked] private TickTimer _delayTimer { get; set; }
    [Networked] private TickTimer _activeTimer { get; set; }

    [Networked, OnChangedRender(nameof(OnHitboxChanged))]
    private NetworkBool _isHitboxActive { get; set; }

    public override void Spawned()
    {
        base.Spawned();
        if (_damageObject != null) _damageObject.SetActive(false);

        if (Object.HasStateAuthority)
        {
            _isHitboxActive = false;
        }
    }

    public override void PerformAttack()
    {
        // Kích hoạt đòn đánh CHỈ trên máy chủ (Master Client)
        if (!Object.HasStateAuthority) return;
        if (!CanAttack()) return;

        // Bắt đầu đếm ngược thời gian vung vũ khí (Start Delay)
        _delayTimer = TickTimer.CreateFromSeconds(Runner, _startDelay);
        ResetCooldown(); // Hàm này nằm ở EnemyAttackBase
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();

        if (!Object.HasStateAuthority) return;

        // GIAI ĐOẠN 1: Hết thời gian chờ (Vung tay xong) -> Bật Hitbox gây damage
        if (_delayTimer.IsRunning && _delayTimer.Expired(Runner))
        {
            _isHitboxActive = true;

            _delayTimer = TickTimer.None; // Tắt đồng hồ delay
            // Lập tức bật đồng hồ duy trì Hitbox
            _activeTimer = TickTimer.CreateFromSeconds(Runner, _hitboxActiveTime);
        }

        // GIAI ĐOẠN 2: Hết thời gian duy trì (Chém xong) -> Tắt Hitbox
        if (_activeTimer.IsRunning && _activeTimer.Expired(Runner))
        {
            _isHitboxActive = false;

            _activeTimer = TickTimer.None; // Tắt đồng hồ duy trì
        }
    }

    public void OnHitboxChanged()
    {
        if (_damageObject != null)
        {
            _damageObject.SetActive(_isHitboxActive);
        }
    }
}