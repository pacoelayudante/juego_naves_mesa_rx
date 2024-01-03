using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StringListWrapper
{
    public static StringListWrapper Empty = new StringListWrapper(new List<string>());

    [SerializeField]
    private List<string> _lista;

    public StringListWrapper(List<string> lista)
    {
        _lista = lista;
    }

    public static implicit operator List<string>(StringListWrapper wrapper) => wrapper == null ? null : wrapper._lista;
    public static implicit operator StringListWrapper(List<string> lista) => lista == null ? null : new StringListWrapper(lista);
}

[DisallowMultipleComponent]
public class PartidaConfigurator : MonoBehaviour
{
    [SerializeField]
    private SelectorConfig _selectConfig;
    [SerializeField]
    private Button _iniciarPartida;

    [SerializeField]
    private DetectorTokensConfigurator _detectorConfig;

    void OnEnable()
    {
        _selectConfig.MostrarOpciones(DetectorTokensConfigurator.ListaDetectores, _selectConfig.OpcionActual);
    }

    void Awake()
    {
        _selectConfig.AlEditarConfig += AbrirEditorDetector;

        _iniciarPartida.onClick.AddListener(IniciarPartida);
        
        _selectConfig.MostrarOpciones(DetectorTokensConfigurator.ListaDetectores, DetectorTokensConfigurator.ConfigSeleccionada);
    }

    void IniciarPartida()
    {
        if (_selectConfig.OpcionActual != null)
        {
            // hacer cuestion
            DetectorTokensConfigurator.ConfigSeleccionada = _selectConfig.OpcionActual;
        }
    }

    void AbrirEditorDetector(string nombre)
    {
        _detectorConfig.gameObject.SetActive(true);
        _detectorConfig.Configurar(nombre);
        gameObject.SetActive(false);

        _detectorConfig.AlGuardarConfiguracion += AlGuardarConfiguracion;
    }

    void AlGuardarConfiguracion(string nombre)
    {
        _detectorConfig.AlGuardarConfiguracion -= AlGuardarConfiguracion;

        _detectorConfig.gameObject.SetActive(false);
        gameObject.SetActive(true);
        _selectConfig.MostrarOpciones(DetectorTokensConfigurator.ListaDetectores, nombre);
    }
}
