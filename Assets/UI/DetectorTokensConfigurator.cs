using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
public class DetectorTokensConfigurator : MonoBehaviour
{
    private const string CONFIG_DETECTOR_SEL = "CONFIG_DETECTOR_SEL";
    private const string CONFIG_DETECTOR_LIST = "CONFIG_DETECTOR_LIST";

    public static StringListWrapper ListaDetectores
    {
        get => JsonUtility.FromJson<StringListWrapper>(PlayerPrefs.GetString(CONFIG_DETECTOR_LIST, "{}"));
        set => PlayerPrefs.SetString(CONFIG_DETECTOR_LIST, JsonUtility.ToJson(value));
    }

    public static string ConfigSeleccionada
    {
        get => PlayerPrefs.GetString(CONFIG_DETECTOR_SEL, null);
        set => PlayerPrefs.SetString(CONFIG_DETECTOR_SEL, value);
    }

    public static ConfigNames LoadConfigNames(string config)
    {
        var nombreLargo = $"{CONFIG_DETECTOR_LIST}.{config}";
        if (PlayerPrefs.HasKey(nombreLargo))
            return JsonUtility.FromJson<ConfigNames>(PlayerPrefs.GetString(nombreLargo, "{}"));

        return null;
    }

    public class ConfigNames
    {
        public string jugA;
        public string jugB;
        public string especiales;
        public string tokens;
        public float minArea;
    }

    public event System.Action<string> AlGuardarConfiguracion;

    [SerializeField]
    SelectorConfig _configJugA;
    [SerializeField]
    SelectorConfig _configJugB;
    [SerializeField]
    SelectorConfig _configEspeciales;
    [SerializeField]
    SelectorConfig _configTokens;
    [SerializeField]
    Slider _minAreaSlider;

    [SerializeField]
    private InputGuardarConfig _guardarButton;
    [SerializeField]
    ColorBlobConfigurator _colorBlobConfig;
    [SerializeField]
    TemplatesConfigurator _templatesConfig;

    ConfigNames _configNames;
    SelectorConfig _ultimoSelectorEditado;

    void Awake()
    {
        _configJugA.AlEditarConfig += (_) => _ultimoSelectorEditado = _configJugA;
        _configJugB.AlEditarConfig += (_) => _ultimoSelectorEditado = _configJugB;
        _configEspeciales.AlEditarConfig += (_) => _ultimoSelectorEditado = _configEspeciales;

        _configJugA.AlEditarConfig += AbrirEditorBlobs;
        _configJugB.AlEditarConfig += AbrirEditorBlobs;
        _configEspeciales.AlEditarConfig += AbrirEditorBlobs;

        _configTokens.AlEditarConfig += AbrirEditorTemplates;

        _guardarButton.OnSave += Guardar;
    }

    void OnEnable()
    {
        _configJugA.MostrarOpciones(ColorBlobConfigurator.ListaConfigBlobs, _configJugA.OpcionActual);
        _configJugB.MostrarOpciones(ColorBlobConfigurator.ListaConfigBlobs, _configJugB.OpcionActual);
        _configEspeciales.MostrarOpciones(ColorBlobConfigurator.ListaConfigBlobs, _configEspeciales.OpcionActual);

        _configTokens.MostrarOpciones(TemplatesConfigurator.ListaTemplates, _configTokens.OpcionActual);
    }

    void AbrirEditorBlobs(string nombre)
    {
        _colorBlobConfig.gameObject.SetActive(true);
        _colorBlobConfig.Configurar(nombre);
        gameObject.SetActive(false);

        _colorBlobConfig.AlGuardarConfiguracion += AlGuardarConfiguracionBlob;
    }

    void AlGuardarConfiguracionBlob(string nombre)
    {
        _colorBlobConfig.AlGuardarConfiguracion -= AlGuardarConfiguracionBlob;

        _colorBlobConfig.gameObject.SetActive(false);
        gameObject.SetActive(true);
        _ultimoSelectorEditado.MostrarOpciones(ColorBlobConfigurator.ListaConfigBlobs, nombre);
    }

    void AbrirEditorTemplates(string nombre)
    {
        _templatesConfig.gameObject.SetActive(true);
        _templatesConfig.Configurar(nombre);
        gameObject.SetActive(false);

        _templatesConfig.AlGuardarConfiguracion += AlGuardarConfiguracionTemplate;
    }

    void AlGuardarConfiguracionTemplate(string nombre)
    {
        _templatesConfig.AlGuardarConfiguracion -= AlGuardarConfiguracionTemplate;

        _templatesConfig.gameObject.SetActive(false);
        gameObject.SetActive(true);
        _configTokens.MostrarOpciones(TemplatesConfigurator.ListaTemplates, nombre);
    }

    public void Configurar(string config)
    {
        if (config == null)
        {
            _configNames = new();
            _guardarButton.Text = string.Empty;
        }
        else
        {
            var nombre = $"{CONFIG_DETECTOR_LIST}.{config}";
            _configNames = JsonUtility.FromJson<ConfigNames>(PlayerPrefs.GetString(nombre, "{}"));
            _guardarButton.Text = config;
        }

        _configJugA.OpcionActual = _configNames.jugA;
        _configJugB.OpcionActual = _configNames.jugB;
        _configEspeciales.OpcionActual = _configNames.especiales;
        _configTokens.OpcionActual = _configNames.tokens;
        _minAreaSlider.value = _configNames.minArea;
    }

    private void Guardar(string nombre)
    {
        if (!string.IsNullOrEmpty(nombre))
        {
            var nombreLargo = $"{CONFIG_DETECTOR_LIST}.{nombre}";
            if (!PlayerPrefs.HasKey(nombreLargo))
            {
                List<string> lista = ListaDetectores;
                lista.Add(nombre);
                ListaDetectores = lista;
            }

            _configNames.jugA = _configJugA.OpcionActual;
            _configNames.jugB = _configJugB.OpcionActual;
            _configNames.especiales = _configEspeciales.OpcionActual;
            _configNames.tokens = _configTokens.OpcionActual;
            _configNames.minArea = _minAreaSlider.value;
            PlayerPrefs.SetString(nombreLargo, JsonUtility.ToJson(_configNames));
        }

        AlGuardarConfiguracion?.Invoke(nombre);
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(DetectorTokensConfigurator))]
    private class DetectorTokensConfiguratorEditor : Editor
    {
        [SerializeField]
        public static TokenDetector _detectorInst;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            foreach (var opcion in (List<string>)ListaDetectores)
            {
                var nombreLargo = $"{CONFIG_DETECTOR_LIST}.{opcion}";
                GUILayout.Label(nombreLargo);
                var json = PlayerPrefs.GetString(nombreLargo, "{}");
                EditorGUILayout.TextArea(json);
                if (GUILayout.Button("Cargar"))
                {
                    ((DetectorTokensConfigurator)target).Configurar(opcion);
                }
                if (GUILayout.Button("Generar"))
                {
                    if (_detectorInst == null)
                    {
                        _detectorInst = ScriptableObject.CreateInstance<TokenDetector>();
                        _detectorInst._blobsAmarillos = ScriptableObject.CreateInstance<ColorBlobs>();
                        _detectorInst._blobsFuxia = ScriptableObject.CreateInstance<ColorBlobs>();
                        _detectorInst._blobsPurpura = ScriptableObject.CreateInstance<ColorBlobs>();
                        _detectorInst._tokenTemplates = ScriptableObject.CreateInstance<TokenTemplates>();
                    }

                    var configNames = JsonUtility.FromJson<ConfigNames>(json);
                    CargarDetector(_detectorInst, configNames);

                    Selection.activeObject = _detectorInst;
                }
            }
        }

        private void CargarDetector(TokenDetector detector, ConfigNames configNames)
        {
            ColorBlobConfigurator.LoadConfiguation(_detectorInst._blobsPurpura, configNames.jugA);
            ColorBlobConfigurator.LoadConfiguation(_detectorInst._blobsAmarillos, configNames.jugB);
            ColorBlobConfigurator.LoadConfiguation(_detectorInst._blobsFuxia, configNames.especiales);
            TemplatesConfigurator.LoadConfiguation(_detectorInst._tokenTemplates, configNames.tokens);
        }
    }
#endif
}
