using UnityEngine;

public class Stove : MonoBehaviour
{
    [SerializeField] GameObject StoveMenu;
    public void OnInteract()
    {
        StoveMenu?.SetActive(true);
    }
}
