using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ChartDemo : MonoBehaviour
{
    [FormerlySerializedAs("_chart")] public RawImage Chart;
    [Range(5, 10)] public int Sides = 10;
    public int Repetitions = 4;

    private float[] array = new float[10];
    private float[] _lastArr = new float[10];
    private float[] lerpedArray = new float[10];

    private float _maxDuration = 2;
    private float _currentDuration;

    private void Start()
    {
        StartCoroutine(ChartUpdate());
    }


    private void Update()
    {
    
        
        _currentDuration += Time.deltaTime;

        for (int i = 0; i < Sides; i++)
        {
            lerpedArray[i] = Mathf.Lerp(_lastArr[i], array[i],
                _currentDuration * _currentDuration * (3.0f - 2.0f * _currentDuration));
        }

        Chart.material.SetFloatArray("_Stats", lerpedArray);
        Chart.material.SetFloat("_Repetitions", Repetitions);
        Chart.material.SetFloat("_Sides", Sides);
    }

    private IEnumerator ChartUpdate()
    {
        while (true)
        {
            _lastArr = (float[])array.Clone();

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = Random.Range(1, Repetitions);
            }

            _currentDuration = 0;
            yield return new WaitForSeconds(1);
        }
    }
}