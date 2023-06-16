using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float rotateDurationSeconds = 0.3f;

    #region Inputs
    private PlayerInput playerInput;
    private InputAction move;
    private InputAction changeProjection;
    private InputAction checkCell;
    private InputAction markCell;
    #endregion
    #region Events
    public event System.Action<Vector3Int> OnMove;
    public event System.Action<Projection> OnProjectionChange;
    #endregion

    private bool isChangingProjection = false;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        move = playerInput.actions["Move"];
        changeProjection = playerInput.actions["Rotate"];
        checkCell = playerInput.actions["Check"];
        markCell = playerInput.actions["Mark"];
    }

    private void OnEnable()
    {
        move.performed += HandleMove;
        changeProjection.performed += HandleProjectionChange;
        checkCell.performed += HandleCheckCell;
        markCell.performed += HandleMarkCell;
    }

    private void OnDisable()
    {
        move.performed -= HandleMove;
        changeProjection.performed -= HandleProjectionChange;
        checkCell.performed -= HandleCheckCell;
        markCell.performed -= HandleMarkCell;
    }

    private void HandleMove(InputAction.CallbackContext context)
    {
        if (!GameManager.Instance.IsGameInProgress || Time.timeScale == 0) return;

        Vector2Int value = Utils.Vector2ToVector2Int(context.ReadValue<Vector2>());
        Vector3 nextPosition = transform.position + MainManager.Instance.ApplyProjection(value);
        Vector3Int roundedNextPosition = new Vector3Int(
            Mathf.RoundToInt(nextPosition.x),
            Mathf.RoundToInt(nextPosition.y),
            Mathf.RoundToInt(nextPosition.z)
        );

        if (!MainManager.Instance.CanPlayerMoveTo(roundedNextPosition)) return;

        transform.position = roundedNextPosition;
        OnMove?.Invoke(roundedNextPosition);
    }

    private void HandleProjectionChange(InputAction.CallbackContext context)
    {
        if (!GameManager.Instance.IsGameInProgress || Time.timeScale == 0) return;

        Vector2Int value = Utils.Vector2ToVector2Int(context.ReadValue<Vector2>());
        if (value.x == 0 && value.y == 0 || isChangingProjection) return;
        isChangingProjection = true;

        Direction direction;
        if (value.x == 1)
        {
            direction = Direction.Right;
        }
        else if (value.x == -1)
        {
            direction = Direction.Left;
        }
        else if (value.y == 1)
        {
            direction = Direction.Top;
        }
        else
        {
            direction = Direction.Bottom;
        }
        MainManager.Instance.ChangeProjection(direction);
        Projection newProjection = MainManager.Instance.currentProjection;
        StartCoroutine(RotateTo(Quaternion.LookRotation(newProjection.ForwardDirection, newProjection.UpwardDirection)));
        OnProjectionChange?.Invoke(newProjection);
    }

    private void HandleCheckCell(InputAction.CallbackContext context)
    {
        if (!GameManager.Instance.IsGameInProgress || Time.timeScale == 0) return;
        MainManager.Instance.CheckCurrentCell();
    }

    private void HandleMarkCell(InputAction.CallbackContext context)
    {
        if (!GameManager.Instance.IsGameInProgress || Time.timeScale == 0) return;
        MainManager.Instance.MarkCurrentCell();
    }

    private IEnumerator RotateTo(Quaternion targetRotation)
    {
        Quaternion startRotation = transform.rotation;

        for (var timePassed = 0.0f; timePassed < rotateDurationSeconds; timePassed += Time.deltaTime)
        {
            float factor = timePassed / rotateDurationSeconds;
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, factor);
            yield return null;
        }

        // just to be sure to end up with clean values
        transform.rotation = targetRotation;
        isChangingProjection = false;
    }
}
