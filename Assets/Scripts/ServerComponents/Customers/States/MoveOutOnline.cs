using Fusion;
using UnityEngine;
using UnityEngine.Events;

public class MoveOutOnline : CustomerStateOnline
{
    [SerializeField] UnityEvent onFinishMovingOut = new();
    [SerializeField] string animatorHorizontalValue = "Horizontal";
    [SerializeField] string animatorVerticalValue = "Vertical";

    [Networked] PathDetails AssignedPathDetails { get; set; }
    [Networked] int CurrentPathPointIndex { get; set; }

    //[Networked] private bool FinishedMovingOut { get; set; }

    private void Start()
    {
        state = CustomerStatesControllerOnline.CustomerStates.MoveOut;
        animatorBool = "IsMoving";
    }
    public override void Init()
    {
    }
    protected override void OnEnter()
    {
        Debug.Log("Customer enters MoveOut state");
        //FinishedMovingOut = false;
        AssignedPathDetails = customer.TablePathDetails;
        CurrentPathPointIndex = AssignedPathDetails.PointCount - 1;
    }

    protected override void UpdateState()
    {
    }

    protected override void RenderState()
    {
        if (customer != null && customer.CustomerAnimator != null && CurrentPathPointIndex < AssignedPathDetails.PointCount)
        {
            var direction = (AssignedPathDetails.Points[CurrentPathPointIndex] - (Vector2)customer.transform.position).normalized;
            customer.CustomerAnimator.SetFloat(animatorHorizontalValue, direction.x);
            customer.CustomerAnimator.SetFloat(animatorVerticalValue, direction.y);
        }
    }

    protected override void FixedUpdateState()
    {
        Move();
    }

    protected override void OnExit()
    {

    }

    public void Move()
    {
        //if (FinishedMovingOut) return;

        if (CurrentPathPointIndex < 0)
        {
            //finishedMovingOut = true;
            onFinishMovingOut.Invoke();
            if (CustomersSpawnerOnline.Instance != null) CustomersSpawnerOnline.Instance.OnCustomerLeft(customer);
            if (controller != null) controller.ChangeState(CustomerStatesControllerOnline.CustomerStates.Null);
            return;
        }

        if (customer.MoveToTarget(AssignedPathDetails.Points[CurrentPathPointIndex])) CurrentPathPointIndex--;
    }
}
