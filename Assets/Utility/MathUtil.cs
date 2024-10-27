using UnityEngine;

public static class MathUtil
{
    public static Vector3Int RoundVector3(Vector3 vector, int ceilSize)
    {
        int xA = Mathf.RoundToInt(vector.x / ceilSize);
        int yA = Mathf.RoundToInt(vector.y);
        int zA = Mathf.RoundToInt(vector.z / ceilSize);

        Vector3Int centerCoordinate = new Vector3Int(xA, yA, zA);

        return centerCoordinate;
    }

    public static Vector2Int RoundVector2(Vector2 vector, int ceilSize)
    {
        int xA = Mathf.RoundToInt(vector.x / ceilSize);
        int zA = Mathf.RoundToInt(vector.y / ceilSize);

        Vector2Int centerCoordinate = new Vector2Int(xA, zA);

        return centerCoordinate;
    }
}
