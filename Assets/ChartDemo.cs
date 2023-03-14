using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ChartDemo : MonoBehaviour
{
    [SerializeField] private RawImage _chart;
    [Range(5, 10)] [SerializeField] private int _sides = 10;
    [SerializeField] private int _repetitions = 4;

    private float[] array = new float[10];
    private float[] _lastArr = new float[10];
    private float[] lerpedArray = new float[10];

    private float _maxDuration = 2;
    private float _currentDuration;

    private void Start()
    {
        StartCoroutine(ChartUpdate());
    }

    private void UpdateArraysSize(int size)
    {
        array = new float[size];
        _lastArr = new float[size];
        lerpedArray = new float[size];
    }

    private void Update()
    {
        if (array.Length != _sides)
        {
            UpdateArraysSize(_sides);
        }
        
        _currentDuration += Time.deltaTime;

        for (int i = 0; i < _sides; i++)
        {
            lerpedArray[i] = Mathf.Lerp(_lastArr[i], array[i],
                _currentDuration * _currentDuration * (3.0f - 2.0f * _currentDuration));
        }

        _chart.material.SetFloatArray("_Stats", lerpedArray);
        _chart.material.SetFloat("_Repetitions", _repetitions);
        _chart.material.SetFloat("_Sides", _sides);
    }

    private IEnumerator ChartUpdate()
    {
        while (true)
        {
            _lastArr = (float[])array.Clone();

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = Random.Range(1, _repetitions);
            }

            _currentDuration = 0;
            yield return new WaitForSeconds(1);
        }
    }
}