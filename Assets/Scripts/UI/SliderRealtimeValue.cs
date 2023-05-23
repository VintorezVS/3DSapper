using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderRealtimeValue : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textObject;
    private Slider slider;

    private void OnEnable()
    {
        slider = GetComponent<Slider>();
        slider?.onValueChanged.AddListener(HandleSliderValueChanged);
        HandleSliderValueChanged(slider.value);
    }

    private void OnDisable()
    {
        slider?.onValueChanged.RemoveListener(HandleSliderValueChanged);
        slider = null;
    }

    private void HandleSliderValueChanged(float value)
    {
        textObject.text = value.ToString();
    }
}
