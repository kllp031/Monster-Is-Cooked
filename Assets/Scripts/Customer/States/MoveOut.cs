using UnityEngine;
using UnityEngine.Events;

public class MoveOut : CustomerState
{
    [SerializeField] UnityEvent onFinishMovingOut = new();
    private Path assignedPath = null;
    private int currentPathPointIndex;
    private bool finishedMovingOut = false;
    public override void Init()
    {
        state = CustomerStatesController.CustomerStates.MoveOut;
        animatorBool = "IsMoving";
    }
    protected override void OnEnter()
    {
        Debug.Log("Customer enters MoveOut state");
        finishedMovingOut = false;
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
        if (finishedMovingOut) return;

        if (currentPathPointIndex < 0) { finishedMovingOut = true; onFinishMovingOut.Invoke() ; return; }

        if (customer.MoveToTarget(assignedPath.Points[currentPathPointIndex])) currentPathPointIndex --;
    }
}
