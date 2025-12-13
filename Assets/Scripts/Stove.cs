using UnityEngine;

public class Stove : MonoBehaviour
{
    [SerializeField] GameObject StoveMenu;
    public void OnInteract(GameObject interactObject)
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.stoveOpen);
        StoveMenu?.SetActive(true);
    }
}
