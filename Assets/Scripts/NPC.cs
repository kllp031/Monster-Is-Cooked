using UnityEngine;

public class NPC : MonoBehaviour, IInteractable
{
    public void OnInteract(GameObject obj)
    {
        if (obj.tag == "Player") Debug.Log("Player makes contact with NPC");
        if (obj.GetComponent<Food>() != null)
        {
            Debug.Log("NPC received food indirectly");
        }
    }
}
