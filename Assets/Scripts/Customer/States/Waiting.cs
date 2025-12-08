using UnityEngine;

public class Waiting : CustomerState
{
    public override void Init()
    {
        state = CustomerStatesController.CustomerStates.Waiting;
        animatorBool = "IsWaiting";
    }
    protected override void OnEnter()
    {
        customer.ReadyToEat();
    }

    protected override void UpdateState()
    {
        if (!customer.IsReadyToEat) controller.ChangeState(CustomerStatesController.CustomerStates.MoveOut);
    }
    
    protected override void FixedUpdateState()
    {

    }
    protected override void OnExit()
    {
        customer.IsReadyToEat = false;
    }
}
