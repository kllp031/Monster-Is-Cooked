using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour, IInteractable
{

    [SerializeField] UnityEvent<GameObject> onInteract;
    public void OnInteract(GameObject interactObject)
    {
        onInteract?.Invoke(interactObject);
    }
}
