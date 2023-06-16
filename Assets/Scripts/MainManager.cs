using System.Collections;
using UnityEngine;
using TMPro;

public class MainManager : MonoBehaviourSingleton<MainManager>
{
    [SerializeField] private Field field;
    [SerializeField] private GameMenuManager gameMenu;
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private TextMeshProUGUI gameOverText;

    public Projection currentProjection { get; private set; } = new Projection();
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
    }

    private void FixedUpdate()
    {
        if (!GameManager.Instance.IsGameInProgress || Time.timeScale == 0) return;

        LevelTime = Time.timeSinceLevelLoad;
    }

    public bool CanPlayerMoveTo(Vector3Int roundedNextPosition)
    {
        return field.IsCellPosition(roundedNextPosition) && field.PlayerPosition != roundedNextPosition;
    }

    public void CheckCurrentCell()
    {
        field.CheckCell();
    }

    public void MarkCurrentCell()
    {
        field.ToggleCellMark();
    }

    public void ChangeProjection(Direction direction)
    {
        currentProjection.Rotate(direction);
    }

    public Vector3Int ApplyProjection(Vector2Int vector)
    {
        return currentProjection.RightwardDirection * vector.x + currentProjection.UpwardDirection * vector.y;
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
}
