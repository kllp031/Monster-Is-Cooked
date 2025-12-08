using UnityEngine;
using UnityEngine.InputSystem.XR;

public class Idle : CustomerState
{
    public override void Init()
    {
        state = CustomerStatesController.CustomerStates.Idle;
        animatorBool = "IsIdle";
    }
    protected override void OnEnter()
    {
    }

    protected override void UpdateState()
    {
        if (customer.IsActivated) controller.ChangeState(CustomerStatesController.CustomerStates.MoveIn);
    }

    protected override void FixedUpdateState()
    {

    }
    protected override void OnExit()
    {
        //customer.IsReadyToEat = false;
    }
}
