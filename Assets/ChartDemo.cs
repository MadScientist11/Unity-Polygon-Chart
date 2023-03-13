using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ChartDemo : MonoBehaviour
{
    [SerializeField] private RawImage _chart;
    [SerializeField] private float _repetitions;

    private float[] array = new float[6];
    private float[] _lastArr = new float[6];
    private float[] lerpedArray = new float[6];

    private float _maxDuration = 2;
    private float _currentDuration;

    private void Start()
    {
        StartCoroutine(ChartUpdate());
    }

    public static float OutElastic(float t)
    {
        float p = 0.3f;
        return (float)Math.Pow(2, -10 * t) * (float)Math.Sin((t - p / 4) * (2 * Math.PI) / p) + 1;
    }

    float easeInOutCirc(float t)
    {
        return t < 0.5f
            ? (float)(1f - Math.Sqrt(1 - Math.Pow(2f * t, 2f))) / 2f
            : (float)(Math.Sqrt(1f - Math.Pow(-2f * t + 2f, 2f)) + 1f) / 2f;
    }

    public static float InElastic(float t) => 1 - OutElastic(1 - t);

    public static float InOutElastic(float t)
    {
        if (t < 0.5) return InElastic(t * 2) / 2;
        return 1 - InElastic((1 - t) * 2) / 2;
    }

    private void Update()
    {
        _currentDuration += Time.deltaTime;

        for (int i = 0; i < 6; i++)
        {
            lerpedArray[i] = Mathf.Lerp(_lastArr[i], array[i],
                _currentDuration * _currentDuration * (3.0f - 2.0f * _currentDuration));
            Debug.Log($"{lerpedArray[i]}");
            Debug.Log($"{_currentDuration}");
        }

        _chart.material.SetFloatArray("_Stats", lerpedArray);


        _chart.material.SetFloat("_Repetitions", _repetitions);
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