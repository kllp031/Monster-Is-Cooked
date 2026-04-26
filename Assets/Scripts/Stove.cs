using UnityEngine;

public class Stove : MonoBehaviour, IInteractable
{
    [SerializeField] GameObject StoveMenu;
    public void OnInteract(GameObject interactObject)
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.stoveOpen);
        StoveMenu?.SetActive(true);
    }
}
