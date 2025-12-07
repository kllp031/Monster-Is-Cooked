using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class Collision2DDetector : MonoBehaviour
{
    [SerializeField] bool detectTrigger = true;
    [SerializeField] bool detectCollision = true;
    [SerializeField] UnityEvent<Collider2D> onTriggerEnter;
    [SerializeField] UnityEvent<Collider2D> onTriggerExit;
    [SerializeField] UnityEvent<Collision2D> onCollisionEnter;
    [SerializeField] UnityEvent<Collision2D> onCollisionExit;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (detectTrigger) onTriggerEnter?.Invoke(collision);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (detectTrigger) onTriggerExit?.Invoke(collision);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (detectCollision) onCollisionEnter?.Invoke(collision);
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (detectCollision) onCollisionExit?.Invoke(collision);
    }
}
