using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class MinMaxUIControl : MonoBehaviour
{
    [SerializeField]
    private TMPro.TMP_Text _label;
    [SerializeField]
    private Slider _minValue;
    [SerializeField]
    private Slider _maxValue;

    public event System.Action<Vector2Int> AlActualizar;
    public event System.Action<Vector2> AlActualizarNormalized;

    public Vector2Int MinMaxValue
    {
        get => new Vector2Int((int)_minValue.value, (int)_maxValue.value);
        set
        {
            _minValue.value = value.x;
            _maxValue.value = value.y;
        }
    }

    public Vector2 MinMaxNormalizedValue
    {
        get => new Vector2(_minValue.normalizedValue, _maxValue.normalizedValue);
        set
        {
            _minValue.normalizedValue = value.x;
            _maxValue.normalizedValue = value.y;
        }
    }

    public void SetMinMaxWithoutNotify(Vector2Int value)
    {
        _minValue.SetValueWithoutNotify(value.x);
        _maxValue.SetValueWithoutNotify(value.y);
    }

    // Start is called before the first frame update
    void Awake()
    {
        AlActualizar += (value) => AlActualizarNormalized?.Invoke(MinMaxNormalizedValue);
        _minValue.onValueChanged.AddListener((value) => AlActualizar?.Invoke(MinMaxValue));
        _maxValue.onValueChanged.AddListener((value) => AlActualizar?.Invoke(MinMaxValue));
    }
}
