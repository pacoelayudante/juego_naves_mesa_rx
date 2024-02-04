using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Rect = UnityEngine.Rect;
using CvRect = OpenCvSharp.Rect;
using OpenCvSharp;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TemplatesConfigurator : MonoBehaviour
{
    private const string CONFIG_TEMPLATES_LIST = "CONFIG_TEMPLATES_LIST";

    public static StringListWrapper ListaTemplates
    {
        get => JsonUtility.FromJson<StringListWrapper>(PlayerPrefs.GetString(CONFIG_TEMPLATES_LIST, "{}"));
        set => PlayerPrefs.SetString(CONFIG_TEMPLATES_LIST, JsonUtility.ToJson(value));
    }

    public static void LoadConfiguation(TokenTemplates configuration, string loadName)
    {
        var nombreLargo = $"{CONFIG_TEMPLATES_LIST}.{loadName}";
        if (PlayerPrefs.HasKey(nombreLargo))
        {
            var json = PlayerPrefs.GetString(nombreLargo, "{}");
            JsonUtility.FromJsonOverwrite(json, configuration);
            configuration.name = loadName;
        }
    }

    [SerializeField]
    SelectorConfig _configToDetect;
    [SerializeField]
    TomarFotoControl _tomarFoto;
    [SerializeField]
    Button _botonExtraerContorno;
    [SerializeField]
    TokenTemplateUI _templateTokenConfigUI;
    [SerializeField]
    InputGuardarConfig _guardarButton;
    [SerializeField]
    ColorBlobConfigurator _colorBlobConfig;

    [SerializeField]
    private TokenTemplates _defaultTokens;
    TokenTemplates _tokensTest;

    [SerializeField]
    ListaEscudos _escudosLevels;

    ColorBlobs _colorBlobParaDetectar;

    Texture2D _resultadoBinarioTex2D;
    public event System.Action<string> AlGuardarConfiguracion;

    private List<TokenTemplateUI> _templatesPool = null;

    void Awake()
    {
        _configToDetect.AlEditarConfig += AbrirEditor;

        _botonExtraerContorno.onClick.AddListener(TemplateDetect);

        _guardarButton.OnSave += Guardar;

        if (_tokensTest == null)
            _tokensTest = ScriptableObject.Instantiate(_defaultTokens);

        UpdateUIList();
    }

    void OnEnable()
    {
        _configToDetect.MostrarOpciones(ColorBlobConfigurator.ListaConfigBlobs, _configToDetect.OpcionActual);
    }

    public void Configurar(string config)
    {
        if (_tokensTest == null)
            _tokensTest = ScriptableObject.Instantiate(_defaultTokens);

        if (config == null)
        {
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(_defaultTokens), _tokensTest);
            _guardarButton.Text = string.Empty;
        }
        else
        {
            var nombreLargo = $"{CONFIG_TEMPLATES_LIST}.{config}";
            var json = PlayerPrefs.GetString(nombreLargo, "{}");
            JsonUtility.FromJsonOverwrite(json, _tokensTest);

            UpdateUIList();

            _guardarButton.Text = config;
        }
    }

    private void Guardar(string nombre)
    {
        if (!string.IsNullOrEmpty(nombre))
        {
            var nombreLargo = $"{CONFIG_TEMPLATES_LIST}.{nombre}";
            if (!PlayerPrefs.HasKey(nombreLargo))
            {
                List<string> lista = ListaTemplates;
                lista.Add(nombre);
                ListaTemplates = lista;
            }

            for (int i = 0; i < _templatesPool.Count && i < _tokensTest.tokenTemplates.Count; i++)
            {
                _templatesPool[i].Apply(_tokensTest.tokenTemplates[i], _escudosLevels);
            }

            _tokensTest.tokenTemplates.RemoveAll(token => token.ordenDeDisparo == -1);
            PlayerPrefs.SetString(nombreLargo, JsonUtility.ToJson(_tokensTest));
        }

        AlGuardarConfiguracion?.Invoke(nombre);
    }

    void OnDestroy()
    {
        if (_resultadoBinarioTex2D != null)
            Destroy(_resultadoBinarioTex2D);
    }

    void TemplateDetect()
    {
        if (CVManager.HsvMat != null)
        {
            var blobDetector = _colorBlobConfig.GenerarScriptableObject(_configToDetect.OpcionActual);
            var hsvMat = CVManager.HsvMat;

            if (blobDetector != null && hsvMat != null && !hsvMat.IsDisposed)
            {
                using (Mat resultadoBinario = new Mat())
                {
                    blobDetector.FromHueMat(CVManager.HsvMat, CVManager.TipoHue, resultadoBinario, out Point[][] contornos, out HierarchyIndex[] jerarquias);

                    Cv2.CvtColor(resultadoBinario, resultadoBinario, ColorConversionCodes.GRAY2BGR);
                    Cv2.DrawContours(resultadoBinario, contornos, -1, Scalar.Red);
                    _resultadoBinarioTex2D = OpenCvSharp.Unity.MatToTexture(resultadoBinario, _resultadoBinarioTex2D);

                    if (_tokensTest == null)
                        _tokensTest = ScriptableObject.Instantiate(_defaultTokens);

                    _tokensTest.tokenTemplates.Clear();
                    //esto esta asi porque es viejo y soy vago
                    _tokensTest.tokenTemplates.AddRange(new TokenTemplates.TokenTemplate[contornos.Length]);

                    float matWidth = CVManager.HsvMat.Width;
                    float matHeight = CVManager.HsvMat.Height;
                    float matAspect = matHeight / matWidth;

                    int siguienteContorno = 0;
                    if (jerarquias.Length > 0)
                    {
                        if (jerarquias[siguienteContorno].Parent != -1)
                            Debug.LogError($"primer contorno tiene parent!");
                        if (jerarquias[siguienteContorno].Previous != -1)
                            Debug.LogError($"primer contorno tiene previous!");
                    }
                    else
                    {
                        siguienteContorno = -1;
                    }

                    while (siguienteContorno != -1)
                    {
                        int i = siguienteContorno;//el siguiente ahora es el actual
                                                  // pero al toque seteo el siguiente por si hago un early continue
                        siguienteContorno = jerarquias[siguienteContorno].Next;
                        // for (int i = 0; i < contornos.Length; i++) // <- usar las jerarquias
                        // {

                        if (contornos[i].Length <= 4) // una lina sin area ni nada muy complicado.. o un punto osea nada que ver
                            continue;

                        while (_templatesPool.Count <= i)
                        {
                            var newDemo = Instantiate(_templateTokenConfigUI, _templateTokenConfigUI.transform.parent);
                            _templatesPool.Add(newDemo);
                        }

                        _tokensTest.tokenTemplates[i] = new TokenTemplates.TokenTemplate()
                        {
                            contorno = contornos[i],
                            cvRect = Cv2.BoundingRect(contornos[i]),
                        };

                        _templatesPool[i].gameObject.SetActive(true);
                        _templatesPool[i].SetImage(_resultadoBinarioTex2D, CVManager.ConvertirBBoxAUVRect(Cv2.BoundingRect(contornos[i]), matWidth, matHeight, Vector4.zero));
                    }
                }
            }

            Destroy(blobDetector);
            UpdateUIList();
        }
    }

    void UpdateUIList()
    {
        if (_tokensTest == null)
            _tokensTest = ScriptableObject.Instantiate(_defaultTokens);

        if (_templatesPool == null)
        {
            _templateTokenConfigUI.gameObject.SetActive(false);
            _templateTokenConfigUI.InitDropdowns(5, _escudosLevels.Count, 2);
            _templatesPool = new();
        }

        foreach (var demo in _templatesPool)
        {
            demo.gameObject.SetActive(false);
        }

        for (int i = 0; i < _tokensTest.tokenTemplates.Count; i++)
        {
            while (_templatesPool.Count <= i)
            {
                var newDemo = Instantiate(_templateTokenConfigUI, _templateTokenConfigUI.transform.parent);
                _templatesPool.Add(newDemo);
            }

            _templatesPool[i].gameObject.SetActive(true);
            _templatesPool[i].Set(_tokensTest.tokenTemplates[i]);

            if (CVManager.HsvMat != null)
            {
                float matWidth = CVManager.HsvMat.Width;
                float matHeight = CVManager.HsvMat.Height;
                _templatesPool[i].SetImage(_resultadoBinarioTex2D, CVManager.ConvertirBBoxAUVRect(_tokensTest.tokenTemplates[i].cvRect, matWidth, matHeight, Vector4.zero));
            }
        }
    }

    void AbrirEditor(string nombre)
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
        _configToDetect.MostrarOpciones(ColorBlobConfigurator.ListaConfigBlobs, nombre);
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(TemplatesConfigurator))]
    private class TemplatesConfiguratorEditor : Editor
    {
        [SerializeField]
        public static TokenTemplates _blobInst;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            foreach (var opcion in (List<string>)ListaTemplates)
            {
                var nombreLargo = $"{CONFIG_TEMPLATES_LIST}.{opcion}";
                GUILayout.Label(nombreLargo);
                var json = PlayerPrefs.GetString(nombreLargo, "{}");
                EditorGUILayout.TextArea(json);
                if (GUILayout.Button("Cargar"))
                {
                    ((TemplatesConfigurator)target).Configurar(opcion);
                }
                if (GUILayout.Button("Generar"))
                {
                    if (_blobInst == null)
                        _blobInst = ScriptableObject.CreateInstance<TokenTemplates>();
                    JsonUtility.FromJsonOverwrite(json, _blobInst);
                    Selection.activeObject = _blobInst;
                }
            }
        }
    }
#endif
}
