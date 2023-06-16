using UnityEngine;

public static class Utils
{
    public static (int, int, int) Vector3ToTuple(Vector3 vector)
    {
        return ((int)vector.x, (int)vector.y, (int)vector.z);
    }

    public static (int, int, int) Vector3ToTuple(Vector3Int vector)
    {
        return (vector.x, vector.y, vector.z);
    }

    public static Vector2Int Vector2ToVector2Int(Vector2 vector)
    {
        return new Vector2Int(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y));
    }

    public static Vector3Int Vector3ToVector3Int(Vector3 vector)
    {
        return new Vector3Int(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y), Mathf.RoundToInt(vector.z));
    }

    public static Vector3Int TupleToVector3Int((int, int, int) tuple)
    {
        return new Vector3Int(tuple.Item1, tuple.Item2, tuple.Item3);
    }

    public static Vector3 MakePositiveVector(Vector3 vector)
    {
        return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
    }
}