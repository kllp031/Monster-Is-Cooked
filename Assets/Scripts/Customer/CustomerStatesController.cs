using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Customer))]
public class CustomerStatesController : MonoBehaviour
{
    [SerializeField] CustomerStates defaultState = CustomerStates.Idle;
    [SerializeField] private CustomerStates currentState;
    private CustomerStates previousState;
    [SerializeField] List<CustomerState> customerStates = new();
    public enum CustomerStates
    {
        Null,
        Idle,
        MoveIn,
        Waiting,
        MoveOut
    }
    public CustomerStates CurrentState { get => currentState; }
    public CustomerStates PreviousState { get => previousState; }

    private void OnEnable()
    {
        customerStates.Clear();
        var states = GetComponents<CustomerState>();
        if (states != null)
        {
            foreach (var state in states)
            {
                if (state != null)
                {
                    state.Init();
                    customerStates.Add(state);
                }
            }
        }

        ChangeState(defaultState);
        previousState = CustomerStates.Null;
    }
    private void Update()
    {
        foreach (var state in customerStates)
        {
            if (state != null) state.ReceiveUpdate(currentState);
        }
    }
    private void FixedUpdate()
    {
        foreach (var state in customerStates)
        {
            if (state != null) state.ReceiveFixedUpdate(currentState);
        }
    }
    private void OnDisable()
    {
        foreach (var state in customerStates)
        {
            if (state != null) state.OnExit(currentState);
        }
    }
    public void ChangeState(CustomerStates newState)
    {
        previousState = currentState;
        currentState = newState;

        AnnounceStateChanged();
    }
    public void ReturnDefaultState()
    {
        previousState = currentState;
        currentState = defaultState;

        AnnounceStateChanged();
    }
    public void ReturnPreviousState()
    {
        CustomerStates tempState = currentState;
        currentState = previousState;
        previousState = tempState;

        AnnounceStateChanged();
    }
    private void AnnounceStateChanged()
    {
        foreach (var state in customerStates)
        {
            if (state != null)
            {
                state.OnExit(previousState);
                state.OnEnter(currentState);
            }
        }
    }
}
