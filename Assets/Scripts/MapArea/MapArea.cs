using UnityEngine;

[CreateAssetMenu(fileName = "MapArea", menuName = "Scriptable Objects/MapArea")]
public class MapArea : ScriptableObject
{
    [SerializeField] private int areaIndex;
    [SerializeField] private Vector2 spawnPosition;
    [SerializeField] private Vector2 topLeft, bottomRight; // Defines the rectangular bounds of the area

    public int getAreaIndex()
    {
        return areaIndex;
    }
    public Vector2 getSpawnPosition()
    {
        return spawnPosition;
    }
    public Vector2 getTopLeft()
    {
        return topLeft;
    }
    public Vector2 getBottomRight()
    {
        return bottomRight;
    }
}
