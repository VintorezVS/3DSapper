using UnityEngine;

public static class Utils
{
    public static (int, int, int) Vector3ToTuple(Vector3 position)
    {
        return ((int)position.x, (int)position.y, (int)position.z);
    }

    public static Vector3 TupleToVector3((int, int, int) position)
    {
        return new Vector3(position.Item1, position.Item2, position.Item3);
    }
}