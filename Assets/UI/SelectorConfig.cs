using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class SelectorConfig : MonoBehaviour
{
    [SerializeField]
    private string _nuevoElementoName = "Crear Nuevo";
    [SerializeField]
    TMP_Dropdown _dropdown;
    [SerializeField]
    Button _configButton;

    public event System.Action<string> AlEditarConfig;

    public string OpcionActual
    {
        get
        {
            if (_dropdown.value == 0)
                return null;
            else
                return _dropdown.options[_dropdown.value].text;
        }
        set
        {
            int indice = _dropdown.options.FindIndex(opcion => opcion.text.Equals(value));

            if (indice < 0)
                indice = 0;

            _dropdown.SetValueWithoutNotify(indice);
        }
    }

    void Awake()
    {
        _configButton.onClick.AddListener(AlBotonConfig);
    }

    public void MostrarOpciones(List<string> opciones, string opcionElegida = null)
    {
        List<TMP_Dropdown.OptionData> opts = new();

        opts.Add(new TMP_Dropdown.OptionData() { text = _nuevoElementoName });
        foreach (var op in opciones)
        {
            opts.Add(new TMP_Dropdown.OptionData() { text = op });
        }

        _dropdown.options = opts;
        _dropdown.SetValueWithoutNotify(opciones.IndexOf(opcionElegida) + 1);
    }

    public void AlBotonConfig()
    {
        AlEditarConfig?.Invoke(OpcionActual);
    }
}
