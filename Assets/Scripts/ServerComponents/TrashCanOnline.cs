using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class TrashCanOnline : NetworkBehaviour, IInteractable
{
    [SerializeField] string eatAnimationTrigger = "Eat";
    [SerializeField] Animator animator;

    // Player tương tác trực tiếp (nhấn interact button khi đứng cạnh thùng rác).
    public void OnInteract(GameObject player)
    {
        var netObj = player.GetComponent<NetworkObject>();
        if (netObj == null || !netObj.HasInputAuthority) return;

        if (player.GetComponent<FoodHolderOnline>() == null) return;
        if (HotbarManagerOnline.Instance == null) return;
        if (HotbarManagerOnline.Instance.CurrentRecipe == null) return;

        HotbarManagerOnline.Instance.RemoveSelectedRecipe();
        RPC_PlayEatAnimation();
    }

    // Food được throw/drop vào vùng thùng rác.
    // OnTriggerEnter2D fire trên mọi client (physics sync qua NetworkRigidbody2D),
    // nên chỉ cần state authority gọi despawn + broadcast animation 1 lần.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        var foodOnline = collision.GetComponent<FoodOnline>();
        if (foodOnline == null) return;
        if (!Object.HasStateAuthority) return;

        foodOnline.RPC_RequestDespawn();
        RPC_PlayEatAnimation();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_PlayEatAnimation()
    {
        if (animator != null) animator.SetTrigger(eatAnimationTrigger);
    }
}
