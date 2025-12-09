using UnityEngine;

public class MoveIn : CustomerState
{
    private Path assignedPath = null;
    private int currentPathPointIndex;
    public override void Init()
    {
        state = CustomerStatesController.CustomerStates.MoveIn;
        animatorBool = "IsMoving";
    }
    protected override void OnEnter()
    {
        Debug.Log("Customer enters MoveIn state");
        assignedPath = customer.TablePath;
        if (assignedPath == null) Debug.LogWarning("Customer doesn't have any table path assigned!");
        currentPathPointIndex = 0;
        if (assignedPath.Points.Count > 0)
        {
            customer.transform.position = assignedPath.Points[0];
        }
    }

    protected override void UpdateState()
    {
    }

    protected override void FixedUpdateState()
    {
        if (assignedPath != null) Move();
    }

    protected override void OnExit()
    {

    }

    public void Move()
    {
        if (currentPathPointIndex >= assignedPath.Points.Count) { Debug.Log("End of points"); controller.ChangeState(CustomerStatesController.CustomerStates.Waiting); return; }

        if (customer.MoveToTarget(assignedPath.Points[currentPathPointIndex])) currentPathPointIndex++;
    }
}
