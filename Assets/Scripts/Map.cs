using UnityEngine;

public class Map : MonoBehaviour
{
    [SerializeField] private Field field;
    [SerializeField] private PlayerController player;
    [SerializeField] private GameObject projectionPlane;
    [SerializeField] private GameObject playerMarker;

    private void OnEnable()
    {
        player.OnMove += HandlePlayerMove;
        player.OnProjectionChange += HandleProjectionChange;
        field.OnReady += HandleFieldReady;
    }

    private void OnDisable()
    {
        player.OnMove -= HandlePlayerMove;
        player.OnProjectionChange -= HandleProjectionChange;
        field.OnReady -= HandleFieldReady;
    }

    private void HandleFieldReady()
    {
        UpdateMap(field.PlayerPosition, MainManager.Instance.currentProjection);
    }

    private void HandleProjectionChange(Projection projection)
    {
        UpdateMap(field.PlayerPosition, projection);
    }

    private void HandlePlayerMove(Vector3Int position)
    {
        UpdateMap(position, MainManager.Instance.currentProjection);
    }

    private void UpdateMap(Vector3 playerPosition, Projection currentProjection)
    {
        Vector3 localPlayerPosition = playerPosition - Vector3.forward * field.Layers / 2;
        Vector3 forwardAxis = Utils.MakePositiveVector(currentProjection.ForwardDirection);
        Vector3 rightwardAxis = Utils.MakePositiveVector(currentProjection.RightwardDirection);
        Vector3 upwardAxis = Utils.MakePositiveVector(currentProjection.UpwardDirection);
        bool isEvenSize = field.Size % 2 == 0;
        float xyStep = 1.0f / field.Size;
        float zStep = 1.0f / field.Layers;
        float rightwardStep = rightwardAxis.z != 0 ? zStep : xyStep;
        float upwardStep = upwardAxis.z != 0 ? zStep : xyStep;
        float forwardStep = forwardAxis.z != 0 ? zStep : xyStep;
        Vector3 forwardOffset = isEvenSize || forwardAxis.z != 0 ? forwardAxis * forwardStep / 2 : Vector3.zero;

        projectionPlane.transform.localPosition = Vector3.Scale(localPlayerPosition, forwardAxis) * forwardStep;
        projectionPlane.transform.localScale = forwardAxis * forwardStep + rightwardAxis + upwardAxis;
        projectionPlane.transform.parent.localPosition = forwardOffset;

        Vector3 rightwardPlayerOffset = rightwardAxis * rightwardStep * (forwardAxis.x == 0 ? 1 : forwardAxis.x) / 2;
        Vector3 upwardPlayerOffset = upwardAxis * upwardStep * (forwardAxis.y == 0 ? 1 : forwardAxis.y) / 2;
        Vector3 playerOffset = rightwardPlayerOffset + upwardPlayerOffset + forwardOffset;
        Vector3 playerParentPosition = new Vector3(localPlayerPosition.x * xyStep, localPlayerPosition.y * xyStep, localPlayerPosition.z * zStep);

        playerMarker.transform.parent.localPosition = playerParentPosition
            + (isEvenSize ? playerOffset : Vector3.Scale(playerOffset, Vector3.forward));
        playerMarker.transform.localRotation = Quaternion.LookRotation(currentProjection.ForwardDirection);
        playerMarker.transform.GetChild(0).transform.localScale = Vector3.up * xyStep + Vector3.right * xyStep
            + Vector3.forward * playerMarker.transform.GetChild(0).transform.localScale.z;
    }
}
