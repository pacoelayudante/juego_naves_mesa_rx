using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class DetectorTokensConfigurator : MonoBehaviour
{
    private const string CONFIG_DETECTOR_LIST = "CONFIG_DETECTOR_LIST";

    public static StringListWrapper ListaDetectores
    {
        get => JsonUtility.FromJson<StringListWrapper>(PlayerPrefs.GetString(CONFIG_DETECTOR_LIST, "{}"));
        set => PlayerPrefs.SetString(CONFIG_DETECTOR_LIST, JsonUtility.ToJson(value));
    }

    public class ConfigNames
    {
        public string jugA;
        public string jugB;
        public string especiales;
        public string tokens;
    }

    [SerializeField]
    SelectorConfig _configJugA;
    [SerializeField]
    SelectorConfig _configJugB;
    [SerializeField]
    SelectorConfig _configEspeciales;
    [SerializeField]
    SelectorConfig _configTokens;

    [SerializeField]
    private InputGuardarConfig _guardarButton;
    [SerializeField]
    ColorBlobConfigurator _colorBlobConfig;

    ConfigNames _configNames;
    SelectorConfig _ultimoSelectorEditado;

    void Awake()
    {
        _configJugA.AlEditarConfig += (_) => _ultimoSelectorEditado = _configJugA;
        _configJugB.AlEditarConfig += (_) => _ultimoSelectorEditado = _configJugB;
        _configEspeciales.AlEditarConfig += (_) => _ultimoSelectorEditado = _configEspeciales;
        _configTokens.AlEditarConfig += (_) => _ultimoSelectorEditado = _configTokens;

        _configJugA.AlEditarConfig += AbrirEditor;
        _configJugB.AlEditarConfig += AbrirEditor;
        _configEspeciales.AlEditarConfig += AbrirEditor;
        _configTokens.AlEditarConfig += AbrirEditor;

        _colorBlobConfig.AlGuardarConfiguracion += AlGuardarConfiguracion;
    }

    void OnEnable()
    {
        _configJugA.MostrarOpciones(ColorBlobConfigurator.ListaConfigBlobs, _configJugA.OpcionActual);
        _configJugB.MostrarOpciones(ColorBlobConfigurator.ListaConfigBlobs, _configJugB.OpcionActual);
        _configEspeciales.MostrarOpciones(ColorBlobConfigurator.ListaConfigBlobs, _configEspeciales.OpcionActual);

        _configTokens.MostrarOpciones(new List<string>(), _configTokens.OpcionActual);
    }

    void AbrirEditor(string nombre)
    {
        _colorBlobConfig.gameObject.SetActive(true);
        _colorBlobConfig.Configurar(nombre);
        gameObject.SetActive(false);
    }

    void AlGuardarConfiguracion(string nombre)
    {
        _colorBlobConfig.gameObject.SetActive(false);
        gameObject.SetActive(true);
        _ultimoSelectorEditado.MostrarOpciones(ColorBlobConfigurator.ListaConfigBlobs, nombre);
    }

    public void Configurar(string config)
    {
        if (_configNames == null)
            _configNames = new();

        if (config == null)
        {
            _configNames = new();
            _guardarButton.Text = string.Empty;
        }
        else
        {
            var nombre = $"{CONFIG_DETECTOR_LIST}.{config}";
            JsonUtility.FromJsonOverwrite(nombre, _configNames);
            _guardarButton.Text = config;
        }

        _configJugA.OpcionActual = _configNames.jugA;
        _configJugB.OpcionActual = _configNames.jugA;
        _configEspeciales.OpcionActual = _configNames.especiales;
        _configTokens.OpcionActual = _configNames.tokens;
    }
}
