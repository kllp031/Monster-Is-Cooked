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
        if(customer.TablePath != null)
        {
            if (customer.TablePath.Points.Count > 0)
            {
                customer.transform.position = customer.TablePath.Points[0];
            }
        }
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
