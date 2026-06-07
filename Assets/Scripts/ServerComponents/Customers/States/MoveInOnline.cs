using Fusion;
using UnityEngine;

public class MoveInOnline : CustomerStateOnline
{
    [SerializeField] string animatorHorizontalValue = "Horizontal";
    [SerializeField] string animatorVerticalValue = "Vertical";

    [Networked] PathDetails AssignedPathDetails { get; set; }
    [Networked] int CurrentPathPointIndex { get; set; }
    private void Start()
    {
        state = CustomerStatesControllerOnline.CustomerStates.MoveIn;
        animatorBool = "IsMoving";
    }
    public override void Init() { }
    protected override void OnEnter()
    {
        Debug.Log("Customer enters MoveIn state");
        AssignedPathDetails = customer.TablePathDetails;
        CurrentPathPointIndex = 0;
        if (AssignedPathDetails.Points.Length > 0)
        {
            customer.transform.position = AssignedPathDetails.Points[0];
        }
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
        if (CurrentPathPointIndex >= AssignedPathDetails.PointCount) { Debug.Log("End of points"); controller.ChangeState(CustomerStatesControllerOnline.CustomerStates.Waiting); return; }
        Debug.Log("Is moving " + AssignedPathDetails.Points[CurrentPathPointIndex] + " index: " + CurrentPathPointIndex);
        if (customer.MoveToTarget(AssignedPathDetails.Points[CurrentPathPointIndex]))
        {
            Debug.Log("Increase index");
            CurrentPathPointIndex++;
        }
    }
}
