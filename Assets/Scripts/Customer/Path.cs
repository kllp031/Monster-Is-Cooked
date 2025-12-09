using System.Collections.Generic;
using UnityEngine;

public class Path : MonoBehaviour
{
    [SerializeField] List<Vector2> points = new();

    public List<Vector2> Points { get => points; }

    [ContextMenu("Load Points From Children")]
    public void LoadPointsFromChildren()
    {
        points.Clear();
        foreach (Transform child in transform)
        {
            points.Add(child.position);
        }
    }

    public void PrintPath()
    {
        string res = string.Empty;
        foreach (var point in points)
        {
            if (point != null)
            {
                if (res != string.Empty)
                {
                    res += " -> ";
                }
                res += point.ToString();
            }
        }
        Debug.Log("Path: " + res);
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
            Gizmos.DrawLine(points[previousPointIndex], points[i]);
            previousPointIndex = i;
        }
    }
}
