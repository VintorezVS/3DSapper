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

    private void OnEnable()
    {
        LoadSavedPrefs();

        fieldSizeSlider.value = fieldSize;
        layersCountSlider.value = layersCount;
        bombsDensitySlider.value = bombsCount * 100 / (fieldSize * fieldSize);

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
        fieldSize = ((int)value);
    }

    private void HandleLayersCountValueChanged(float value)
    {
        layersCount = ((int)value);
    }

    private void HandleBombsDensityValueChanged(float value)
    {
        bombsCount = Mathf.RoundToInt(fieldSize * fieldSize * value / 100);
    }

    private void LoadSavedPrefs()
    {
        fieldSize = PlayerPrefs.GetInt(Constants.FIELD_SIZE, fieldSize);
        layersCount = PlayerPrefs.GetInt(Constants.LAYERS_COUNT, layersCount);
        bombsCount = PlayerPrefs.GetInt(Constants.BOMBS_COUNT, bombsCount);
    }

    public void StartGame()
    {
        GameManager.Instance.StartGame();
        PlayerPrefs.SetInt(Constants.FIELD_SIZE, fieldSize);
        PlayerPrefs.SetInt(Constants.LAYERS_COUNT, layersCount);
        PlayerPrefs.SetInt(Constants.BOMBS_COUNT, bombsCount);
    }
}
