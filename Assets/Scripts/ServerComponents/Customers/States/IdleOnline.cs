using Fusion;
using UnityEngine;

public class IdleOnline : CustomerStateOnline
{
    public override void Init() { }
    private void Start()
    {
        state = CustomerStatesControllerOnline.CustomerStates.Idle;
        animatorBool = "IsIdle";
    }
    protected override void OnEnter()
    {
        //Debug.Log("Idle enter: " + customer.TablePathDetails.PointCount);
        if (customer.TablePathDetails.PointCount > 0)
        {
            //Debug.Log("Idle enter: " + customer.TablePathDetails.Points[0]);
            if (customer.TryGetComponent(out NetworkTransform networkTransform))
            {
                networkTransform.Teleport(customer.TablePathDetails.Points[0], Quaternion.identity);
            }
            //customer.transform.position = customer.TablePathDetails.Points[0];
        }
    }

    protected override void UpdateState()
    {

    }

    protected override void FixedUpdateState()
    {
        if (customer.IsActivated) controller.ChangeState(CustomerStatesControllerOnline.CustomerStates.MoveIn);
    }

    protected override void RenderState()
    {
        
    }

    protected override void OnExit()
    {
        //customer.IsReadyToEat = false;
    }
}
