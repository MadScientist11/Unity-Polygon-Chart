using UnityEngine;
using UnityEngine.UI;

public class ChartSettingsUI : MonoBehaviour
{
    [SerializeField] private ChartDemo _chartDemo;
    
    [SerializeField] private Slider _sidesSlider;
    [SerializeField] private Slider _repetitionsSlider;
    [SerializeField] private Slider _statCirclesSlider;

    private void OnEnable()
    {
        ChangeSidesNumber(_sidesSlider.value);
        ChangeRepetitionsNumber(_repetitionsSlider.value);
        ChangeStatCircles(_statCirclesSlider.value);
        _sidesSlider.onValueChanged.AddListener(ChangeSidesNumber);
        _repetitionsSlider.onValueChanged.AddListener(ChangeRepetitionsNumber);
        _statCirclesSlider.onValueChanged.AddListener(ChangeStatCircles);
    }

    private void OnDisable()
    {
        _sidesSlider.onValueChanged.RemoveListener(ChangeSidesNumber);
        _repetitionsSlider.onValueChanged.RemoveListener(ChangeRepetitionsNumber);
        _statCirclesSlider.onValueChanged.RemoveListener(ChangeStatCircles);
    }

    private void ChangeRepetitionsNumber(float value)
    {
        _chartDemo.Repetitions = (int)value;
    }

    private void ChangeSidesNumber(float value)
    {
        _chartDemo.Sides = (int)value;
    }
    
    private void ChangeStatCircles(float value)
    {
        _chartDemo.Chart.material.SetFloat("_StatCircles", value);
    }
}
