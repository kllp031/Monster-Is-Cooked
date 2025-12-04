using UnityEngine;

public class EmptyState : CustomerState
{
    public override void Init()
    {
        state = CustomerStatesController.CustomerStates.Null;
    }
    protected override void FixedUpdateState()
    {
    }

    protected override void OnEnter()
    {
        Debug.Log("Customer enters an empty state!");
    }

    protected override void OnExit()
    {
    }

    protected override void UpdateState()
    {
    }

}
