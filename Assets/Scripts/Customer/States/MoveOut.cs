using UnityEngine;

public class MoveOut : CustomerState
{
    private Path assignedPath = null;
    private int currentPathPointIndex;
    public override void Init()
    {
        state = CustomerStatesController.CustomerStates.MoveOut;
    }
    protected override void OnEnter()
    {
        Debug.Log("Customer enters MoveOut state");
        assignedPath = customer.TablePath;
        if (assignedPath == null) Debug.LogWarning("Customer doesn't have any table path assigned!");
        currentPathPointIndex = assignedPath.Points.Count - 1;
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
        if (currentPathPointIndex < 0) { Debug.Log("End of points"); controller.ChangeState(CustomerStatesController.CustomerStates.Idle); return; }

        if (customer.MoveToTarget(assignedPath.Points[currentPathPointIndex])) currentPathPointIndex --;
    }
}
