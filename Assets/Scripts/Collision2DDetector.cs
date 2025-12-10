using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class Collision2DDetector : MonoBehaviour
{
    [SerializeField] bool useOverlap = false;
    [SerializeField] LayerMask overlapMask;
    [SerializeField] float overlapRange;
    [SerializeField] Vector2 overlapOffset = new();

    [SerializeField] bool detectTrigger = true;
    [SerializeField] bool detectCollision = true;
    [SerializeField] UnityEvent<Collider2D[]> onOverlapDetected = new();
    [SerializeField] UnityEvent<Collider2D> onTriggerEnter;
    [SerializeField] UnityEvent<Collider2D> onTriggerStay;
    [SerializeField] UnityEvent<Collider2D> onTriggerExit;
    [SerializeField] UnityEvent<Collision2D> onCollisionEnter;
    [SerializeField] UnityEvent<Collision2D> onCollisionExit;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (detectTrigger && !useOverlap) onTriggerEnter?.Invoke(collision);
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (detectTrigger && !useOverlap) onTriggerStay?.Invoke(collision);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (detectTrigger && !useOverlap) onTriggerExit?.Invoke(collision);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (detectTrigger && !useOverlap) onCollisionEnter?.Invoke(collision);
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (detectTrigger && !useOverlap) onCollisionExit?.Invoke(collision);
    }

    private void FixedUpdate()
    {
        if (!useOverlap) return;
        var colliders = Physics2D.OverlapCircleAll((Vector2)transform.position + overlapOffset, overlapRange, overlapMask);
        if(colliders != null) onOverlapDetected?.Invoke(colliders);
    }
}
