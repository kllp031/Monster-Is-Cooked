using UnityEngine;
using Fusion; 

public class BulletMove : NetworkBehaviour 
{
    [Header("Settings")]
    public float speed = 10f;

    [Networked]
    public Vector2 direction { get; set; } = Vector2.right;

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();

        transform.position += (Vector3)(direction * speed * Runner.DeltaTime);
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }
}