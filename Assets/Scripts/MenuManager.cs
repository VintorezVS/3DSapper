using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Slider fieldSizeSlider;
    [SerializeField] private Slider layersCountSlider;
    [SerializeField] private Slider bombsDensitySlider;
    private int fieldSize = 10;
    private int layersCount = 2;
    private int bombsCount = 20;

    private void Start()
    {
        LoadSavedPrefs();
        SetInitialValues();
    }

    private void OnEnable()
    {
        fieldSizeSlider?.onValueChanged.AddListener(HandleFieldSizeValueChanged);
        layersCountSlider?.onValueChanged.AddListener(HandleLayersCountValueChanged);
        bombsDensitySlider?.onValueChanged.AddListener(HandleBombsDensityValueChanged);
    }

    private void OnDisable()
    {
        fieldSizeSlider?.onValueChanged.RemoveListener(HandleFieldSizeValueChanged);
        layersCountSlider?.onValueChanged.RemoveListener(HandleLayersCountValueChanged);
        bombsDensitySlider?.onValueChanged.RemoveListener(HandleBombsDensityValueChanged);
    }

    private void HandleFieldSizeValueChanged(float value)
    {
        fieldSize = ClampFieldSize((int)value);
        HandleBombsDensityValueChanged(bombsDensitySlider.value);
    }

    private void HandleLayersCountValueChanged(float value)
    {
        layersCount = ClampLayersCount((int)value);
        HandleBombsDensityValueChanged(bombsDensitySlider.value);
    }

    private void HandleBombsDensityValueChanged(float value)
    {
        bombsCount = ClampBombsCount(Mathf.RoundToInt(layersCount * fieldSize * fieldSize * value / 100));
    }

    private void LoadSavedPrefs()
    {
        fieldSize = ClampFieldSize(PlayerPrefs.GetInt(Constants.FIELD_SIZE, fieldSize));
        layersCount = ClampLayersCount(PlayerPrefs.GetInt(Constants.LAYERS_COUNT, layersCount));
        bombsCount = ClampBombsCount(PlayerPrefs.GetInt(Constants.BOMBS_COUNT, bombsCount));
    }

    public void StartGame()
    {
        PlayerPrefs.SetInt(Constants.FIELD_SIZE, fieldSize);
        PlayerPrefs.SetInt(Constants.LAYERS_COUNT, layersCount);
        PlayerPrefs.SetInt(Constants.BOMBS_COUNT, bombsCount);
        GameManager.Instance.StartGame();
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    private void SetInitialValues()
    {
        fieldSizeSlider.gameObject.SetActive(false);
        layersCountSlider.gameObject.SetActive(false);
        bombsDensitySlider.gameObject.SetActive(false);

        fieldSizeSlider.SetValueWithoutNotify(fieldSize);
        layersCountSlider.SetValueWithoutNotify(layersCount);
        bombsDensitySlider.SetValueWithoutNotify(bombsCount * 100 / (fieldSize * fieldSize * layersCount));

        fieldSizeSlider.gameObject.SetActive(true);
        layersCountSlider.gameObject.SetActive(true);
        bombsDensitySlider.gameObject.SetActive(true);
    }

    private int ClampFieldSize(int fieldSize)
    {
        return Mathf.Clamp(fieldSize, 5, 30);
    }

    private int ClampLayersCount(int layersCount)
    {
        return Mathf.Clamp(layersCount, 2, 5);
    }

    private int ClampBombsCount(int bombsCount)
    {
        return Mathf.Clamp(bombsCount, 2, 3150);
    }
}
