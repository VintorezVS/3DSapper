using UnityEngine;

public class Projection
{
    public Vector3Int ForwardDirection { get; private set; } = Vector3Int.forward;
    public Vector3Int UpwardDirection { get; private set; } = Vector3Int.up;
    public Vector3Int RightwardDirection { get; private set; } = Vector3Int.right;

    public void Rotate(Direction direction)
    {
        Vector3Int previousForwardDirection = ForwardDirection;
        switch (direction)
        {
            case Direction.Top:
                {
                    ForwardDirection = UpwardDirection * -1;
                    UpwardDirection = previousForwardDirection;
                    break;
                }
            case Direction.Bottom:
                {
                    ForwardDirection = UpwardDirection;
                    UpwardDirection = previousForwardDirection * -1;
                    break;
                }
            case Direction.Right:
                {
                    ForwardDirection = RightwardDirection * -1;
                    RightwardDirection = previousForwardDirection;
                    break;
                }
            case Direction.Left:
                {
                    ForwardDirection = RightwardDirection;
                    RightwardDirection = previousForwardDirection * -1;
                    break;
                }
        }
    }
}