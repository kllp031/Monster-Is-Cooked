using UnityEngine;

public class Stove : MonoBehaviour
{
    [SerializeField] GameObject StoveMenu;
    public void OnInteract(GameObject interactObject)
    {
        StoveMenu?.SetActive(true);
    }
}
