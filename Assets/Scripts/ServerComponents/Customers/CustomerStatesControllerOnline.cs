using Fusion;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CustomerOnline))]
public class CustomerStatesControllerOnline : NetworkBehaviour
{
    [SerializeField] CustomerStates defaultState = CustomerStates.Idle;
    [Networked] public CustomerStates CurrentState { get; private set; }
    [Networked] CustomerStates PreviousState { get; set; }
    [SerializeField] List<CustomerStateOnline> customerStates = new();
    public enum CustomerStates
    {
        Null,
        Idle,
        MoveIn,
        Waiting,
        MoveOut
    }

    public override void Spawned()
    {
        customerStates.Clear();
        var states = GetComponents<CustomerStateOnline>();
        if (states != null)
        {
            foreach (var state in states)
            {
                if (Runner != null && Runner.IsSharedModeMasterClient) state.Init();
                customerStates.Add(state);
            }
        }

        OnReset();
    }

    private void OnReset()
    {
        if (Runner != null && Runner.IsSharedModeMasterClient)
        {
            ChangeState(defaultState);
            PreviousState = CustomerStates.Null;
        }
    }

    private void Update()
    {
        if (Runner == null || !Runner.IsSharedModeMasterClient) return;
        foreach (var state in customerStates)
        {
            if (state != null) state.ReceiveUpdate(CurrentState);
        }
    }

    public override void Render()
    {
        foreach (var state in customerStates)
        {
            if (state != null) state.ReceiveRender(CurrentState);
        }
    }

    public override void FixedUpdateNetwork()
    {
        //if (Runner == null || !Runner.IsSharedModeMasterClient) return;
        foreach (var state in customerStates)
        {
            if (state != null)
            {
                if (Runner != null && Runner.IsSharedModeMasterClient) state.ReceiveFixedUpdate(CurrentState);
                state.ReceiveFixedUpdate(CurrentState);
            }
        }
    }
    //private void FixedUpdate()
    //{
    //    if (Runner == null || !Runner.IsSharedModeMasterClient) return;
    //    foreach (var state in customerStates)
    //    {
    //        if (state != null) state.ReceiveFixedUpdate(CurrentState);
    //    }
    //}
    private void OnDisable()
    {
        if (Runner == null || !Runner.IsSharedModeMasterClient) return;
        foreach (var state in customerStates)
        {
            if (state != null) state.OnExit(CurrentState);
        }
    }
    public void ChangeState(CustomerStates newState)
    {
        //Debug.Log("Change state: " + newState);
        if (Runner == null || !Runner.IsSharedModeMasterClient) return;
        PreviousState = CurrentState;
        CurrentState = newState;

        AnnounceStateChanged();
    }
    public void ReturnDefaultState()
    {
        if (Runner == null || !Runner.IsSharedModeMasterClient) return;
        PreviousState = CurrentState;
        CurrentState = defaultState;

        AnnounceStateChanged();
    }
    public void ReturnPreviousState()
    {
        if (Runner == null || !Runner.IsSharedModeMasterClient) return;
        CustomerStates tempState = CurrentState;
        CurrentState = PreviousState;
        PreviousState = tempState;

        AnnounceStateChanged();
    }
    private void AnnounceStateChanged()
    {
        if (Runner == null || !Runner.IsSharedModeMasterClient) return;
        //Debug.Log("Announce state changed: " + CurrentState + " prev: " + PreviousState);
        foreach (var state in customerStates)
        {
            if (state != null)
            {
                state.OnExit(PreviousState);
                state.OnEnter(CurrentState);
            }
        }
    }
}
