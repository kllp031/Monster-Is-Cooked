using System;
using UnityEngine;

[RequireComponent(typeof(CustomerStatesController))]
public abstract class CustomerState : MonoBehaviour
{
    [SerializeField] protected CustomerStatesController.CustomerStates state;
    [SerializeField] protected string animatorTrigger;
    protected Customer customer;
    protected Animator customerAnimator;
    protected CustomerStatesController controller;

    // These functions are called by the CustomerStatesController
    public void OnEnter(CustomerStatesController.CustomerStates state)
    {
        if (this.state == state)
        {
            customer = GetComponent<Customer>();
            controller = GetComponent<CustomerStatesController>();
            SetAnimator();
            OnEnter();
        }
    }
    public void ReceiveUpdate(CustomerStatesController.CustomerStates state)
    {
        if (this.state == state) UpdateState();
    }
    public void ReceiveFixedUpdate(CustomerStatesController.CustomerStates state)
    {
        if (this.state == state) FixedUpdateState();
    }
    public void OnExit(CustomerStatesController.CustomerStates state)
    {
        if (this.state == state) OnExit();
    }

    // These functions are to be implemented by the specific states
    public abstract void Init();
    protected abstract void OnEnter();
    protected abstract void UpdateState();
    protected abstract void FixedUpdateState();
    protected abstract void OnExit();

    protected void SetAnimator()
    {
        customerAnimator = customer.CustomerAnimator;
        if (customerAnimator == null) Debug.LogWarning("Customer doesn't have any animator assigned!");
        else customerAnimator.SetTrigger(animatorTrigger);
    }
}
