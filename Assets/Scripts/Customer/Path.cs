using System.Collections.Generic;
using UnityEngine;

public class Path : MonoBehaviour
{
    [SerializeField] List<Vector2> points = new();

    [ContextMenu("Load Points From Children")]
    public void LoadPointsFromChildren()
    {
        points.Clear();
        foreach (Transform child in transform)
        {
            points.Add(child.localPosition);
        }
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        int previousPointIndex = 0;
        for (int i = 1; i < points.Count; i++)
        {
            if (points[previousPointIndex] == null)
            {
                previousPointIndex = i;
                continue;
            }
            if (points[i] == null)
            {
                continue;
            }
            Gizmos.DrawLine(points[previousPointIndex] + (Vector2)transform.position, points[i] + (Vector2)transform.position);
            previousPointIndex = i;
        }
    }
}
