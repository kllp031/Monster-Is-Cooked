using UnityEngine;

public class NPC : MonoBehaviour
{
    public void OnInteract(GameObject obj)
    {
        print("Interacting with NPC: " + gameObject.name);
    }
}
