using System.Collections;
using UnityEngine;
using TMPro;

public class MainManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Field field;
    [SerializeField] private PlayerController player;
    [SerializeField] private GameMenuManager gameMenu;
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private TextMeshProUGUI time;

    public float LevelTime { get; private set; } = 0;

    private void OnEnable()
    {
        GameManager.Instance.OnGameOver += HandleGameOver;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnGameOver -= HandleGameOver;
    }

    void Start()
    {
        field.Generate(
            PlayerPrefs.GetInt(Constants.FIELD_SIZE),
            PlayerPrefs.GetInt(Constants.LAYERS_COUNT),
            PlayerPrefs.GetInt(Constants.BOMBS_COUNT)
        );
    }

    private void Update()
    {
        if (!GameManager.Instance.IsGameInProgress) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameMenu.ToggleMenu();
        }

        if (Time.timeScale == 0) return;

        bool isLeftClick = Input.GetMouseButtonDown(0);
        bool isRightClick = Input.GetMouseButtonDown(1);

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

    private void FixedUpdate()
    {
        if (!GameManager.Instance.IsGameInProgress || Time.timeScale == 0) return;

        LevelTime = Time.timeSinceLevelLoad;
        time.text = LevelTime.ToString("0.00");
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

    private void HandleGameOver(bool isWin)
    {
        StartCoroutine(ShowGameOverMenu(isWin));
    }

    private IEnumerator ShowGameOverMenu(bool isWin)
    {
        yield return new WaitForSeconds(0.5f);
        string finalText = isWin ? "Level passed" : "Level failed";
        gameOverText.text = $"<b>{finalText}</b>\nTime: {LevelTime:0.00}";
        gameOverMenu.SetActive(true);
    }

    private Projection GetProjectionByAngle(int angle)
    {
        angle %= 360;
        if (angle == 270) angle = -90;
        return (Projection)angle;
    }
}
