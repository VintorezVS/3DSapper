using UnityEngine;

public class MainManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject fieldObject;
    [SerializeField] private GameObject playerObject;
    private Field field;
    private PlayerController player;

    void Start()
    {
        field = fieldObject.GetComponent<Field>();
        player = playerObject.GetComponent<PlayerController>();
        field.PlayerPosition = playerObject.transform.position;
        field.Generate();
    }

    private void Update()
    {
        if (!GameManager.Instance.IsGameInProgress) return;

        bool isLeftClick = Input.GetMouseButtonDown(0);
        bool isRightClick = Input.GetMouseButtonDown(2);

        if (isLeftClick || isRightClick)
        {
            Vector3 cellPosition = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.farClipPlane));
            Vector3 roundedCellPosition = new Vector3(Mathf.Round(cellPosition.x), Mathf.Round(cellPosition.y), Mathf.Round(cellPosition.z));

            if (!field.IsCellPosition(roundedCellPosition) || field.PlayerPosition == roundedCellPosition) return;

            if (isRightClick)
            {
                field.ToggleCellMark(roundedCellPosition);
            }
            else if (Vector3.Distance(field.PlayerPosition, roundedCellPosition) <= 1.5 && !field.IsCellMarked(roundedCellPosition))
            {
                field.MovePlayerTo(roundedCellPosition, GetProjectionByAngle((int)player.transform.rotation.eulerAngles.y));
                player.MoveTo(roundedCellPosition);
            }
        }
    }

    public void ChangeProjectionToLeft()
    {
        UpdateField(Projection.Left);
        player.ChangeProjectionToLeft();
    }

    public void ChangeProjectionToRight()
    {
        UpdateField(Projection.Right);
        player.ChangeProjectionToRight();
    }

    private void UpdateField(Projection projection)
    {
        field.OnProjectionChange(GetProjectionByAngle((int)player.transform.rotation.eulerAngles.y + ((int)projection)));
    }

    private Projection GetProjectionByAngle(int angle)
    {
        angle %= 360;
        if (angle == 270) angle = -90;
        return (Projection)angle;
    }
}
