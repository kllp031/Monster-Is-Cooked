using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion; 

public class TimedObjectDestroyer : NetworkBehaviour
{
    [Header("Settings:")]
    [Tooltip("The lifetime of this gameobject in seconds")]
    public float lifetime = 5.0f;

    [Tooltip("Whether or not to destroy child gameobjects when this gameobject is destroyed")]
    public bool destroyChildrenOnDeath = true;

    // Sử dụng TickTimer để đồng bộ thời gian hủy tuyệt đối chính xác qua internet
    [Networked] private TickTimer _lifetimeTimer { get; set; }

    // Dùng cho trường hợp đồ họa local không có kết nối mạng (Bọc mỏ neo an toàn)
    private float _localTimeAlive = 0f;

    // Thay thế Start() bằng Spawned() để kích hoạt bộ đếm thời gian mạng ngay khi xuất hiện
    public override void Spawned()
    {
        base.Spawned();

        // Chỉ máy nắm quyền điều khiển đối tượng (Master Client) mới được đặt giờ hủy mạng
        if (Object != null && Object.HasStateAuthority)
        {
            _lifetimeTimer = TickTimer.CreateFromSeconds(Runner, lifetime);
        }

        _localTimeAlive = 0f;
    }

    // Logic kiểm tra hủy mạng đặt trong FixedUpdateNetwork (FUN)
    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();

        // TRƯỜNG HỢP 1: Đây là một đối tượng kết nối mạng (NetworkObject)
        if (Object != null)
        {
            // Chỉ Trọng tài (State Authority) mới có quyền ra lệnh xóa đối tượng khỏi phòng chơi
            if (Object.HasStateAuthority)
            {
                if (_lifetimeTimer.Expired(Runner))
                {
                    DoDeathContext();
                }
            }
        }
    }

    // Luồng Update thường chỉ phục vụ cho các hiệu ứng hạt local (VFX chớp đỏ, khói bếp dải local)
    void Update()
    {
        // TRƯỜNG HỢP 2: Đây là đối tượng đồ họa local thuần túy (Instantiate thủ công, Object mạng bằng null)
        if (Object == null)
        {
            _localTimeAlive += Time.deltaTime;
            if (_localTimeAlive >= lifetime)
            {
                DoDeathContext();
            }
        }
    }

    /// <summary>
    /// Xử lý dọn dẹp vòng đời đối tượng thích ứng theo môi trường mạng/cục bộ
    /// </summary>
    private void DoDeathContext()
    {
        // Nếu là đối tượng mạng, thực hiện Despawn truyền tin toàn phòng
        if (Object != null && Object.HasStateAuthority)
        {
            Runner.Despawn(Object);
        }
        // Nếu là đối tượng local, dùng Destroy thường để giải phóng RAM máy local
        else if (Object == null)
        {
            Destroy(this.gameObject);
        }
    }

    // Cờ báo ngắt ứng dụng hỗ trợ tránh lỗi Null khi tắt Engine
    public static bool quitting = false;

    private void OnApplicationQuit()
    {
        quitting = true;
    }

    // Tận dụng hàm kết thúc vòng đời tích hợp sẵn của Fusion 2
    public override void Despawned(NetworkRunner runner, bool hasStateAuthority)
    {
        base.Despawned(runner, hasStateAuthority);
        HandleChildrenCleanup();
    }

    private void OnDestroy()
    {
        HandleChildrenCleanup();
    }

    /// <summary>
    /// Logic dọn dẹp các GameObject con đính kèm từ đồ án cũ
    /// </summary>
    private void HandleChildrenCleanup()
    {
        if (destroyChildrenOnDeath && !quitting && Application.isPlaying)
        {
            int childCount = transform.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                if (transform.GetChild(i) != null)
                {
                    GameObject childObject = transform.GetChild(i).gameObject;
                    if (childObject != null)
                    {
                        // Nếu con là đối tượng vật lý mạng thường, để Runner tự quản lý,
                        // Ở đây chỉ Destroy các thành phần Sprite/UI con local đính kèm
                        Destroy(childObject);
                    }
                }
            }
        }
        transform.DetachChildren();
    }
}