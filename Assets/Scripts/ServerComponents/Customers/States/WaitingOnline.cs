using UnityEngine;

public class WaitingOnline : CustomerStateOnline
{
    private void Start()
    {
        state = CustomerStatesControllerOnline.CustomerStates.Waiting;
        animatorBool = "IsWaiting";
    }

    public override void Init()
    {
    }
    protected override void OnEnter()
    {
        //Debug.Log("Customer enters Waiting state");
        customer.ReadyToEat();
    }

    protected override void UpdateState()
    {
    }

    protected override void FixedUpdateState()
    {
        if (!customer.IsReadyToEat) controller.ChangeState(CustomerStatesControllerOnline.CustomerStates.MoveOut);
    }

    protected override void RenderState()
    {

    }

    protected override void OnExit()
    {
        customer.IsReadyToEat = false;
    }
}
