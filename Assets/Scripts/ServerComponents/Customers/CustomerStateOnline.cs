using Fusion;
using UnityEngine;

[RequireComponent(typeof(CustomerStatesControllerOnline))]
public abstract class CustomerStateOnline : NetworkBehaviour
{
    [SerializeField] protected CustomerStatesControllerOnline.CustomerStates state;
    [SerializeField] protected string animatorBool;
    protected CustomerOnline customer;
    protected Animator customerAnimator;
    protected CustomerStatesControllerOnline controller;

    private void OnEnable()
    {
        customer = GetComponent<CustomerOnline>();
        controller = GetComponent<CustomerStatesControllerOnline>();        
    }

    // These functions are called by the CustomerStatesController
    public void OnEnter(CustomerStatesControllerOnline.CustomerStates state)
    {
        if (this.state == state)
        {
            AnnounceSetAnimator();
            OnEnter();
        }
    }
    public void ReceiveUpdate(CustomerStatesControllerOnline.CustomerStates state)
    {
        if (this.state == state) UpdateState();
    }
    public void ReceiveFixedUpdate(CustomerStatesControllerOnline.CustomerStates state)
    {
        if (this.state == state) FixedUpdateState();
    }
    public void ReceiveRender(CustomerStatesControllerOnline.CustomerStates state)
    {
        if (this.state == state) RenderState();
    }
    public void OnExit(CustomerStatesControllerOnline.CustomerStates state)
    {
        if (this.state == state)
        {
            AnnounceUnsetAnimator();
            OnExit();
        }
    }

    // These functions are to be implemented by the specific states
    public abstract void Init();
    protected abstract void OnEnter();
    protected abstract void UpdateState();
    protected abstract void FixedUpdateState();
    protected abstract void RenderState();
    protected abstract void OnExit();

    protected void AnnounceSetAnimator()
    {
        if (Runner != null && Runner.IsSharedModeMasterClient) RPC_SetAnimator();
    }

    protected void AnnounceUnsetAnimator()
    {
        if (Runner != null && Runner.IsSharedModeMasterClient) RPC_UnsetAnimator();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    protected void RPC_SetAnimator()
    {
        customerAnimator = customer.CustomerAnimator;
        //if (customerAnimator == null) Debug.LogWarning("Customer doesn't have any animator assigned!");
        //else customerAnimator.SetTrigger(animatorTrigger);
        if (customerAnimator != null && !string.IsNullOrEmpty(animatorBool)) customerAnimator.SetBool(animatorBool, true);
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    protected void RPC_UnsetAnimator()
    {
        customerAnimator = customer.CustomerAnimator;
        //if (customerAnimator == null) Debug.LogWarning("Customer doesn't have any animator assigned!");
        if (customerAnimator != null && !string.IsNullOrEmpty(animatorBool)) customerAnimator.SetBool(animatorBool, false);
    }
}
