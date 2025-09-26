using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{

    [SerializeField] UnityEvent onInteract;
    public void OnInteract()
    {
        onInteract?.Invoke();
    }
}
