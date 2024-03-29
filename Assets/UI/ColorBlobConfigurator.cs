using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCvSharp;
#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
public class ColorBlobConfigurator : MonoBehaviour
{
    private const string CONFIG_BLOBS_LIST = "CONFIG_BLOBS_LIST";

    public static StringListWrapper ListaConfigBlobs
    {
        get => JsonUtility.FromJson<StringListWrapper>(PlayerPrefs.GetString(CONFIG_BLOBS_LIST, "{}"));
        set => PlayerPrefs.SetString(CONFIG_BLOBS_LIST, JsonUtility.ToJson(value));
    }

    public static void LoadConfiguation(ColorBlobs configuration, string loadName)
    {
        var nombreLargo = $"{CONFIG_BLOBS_LIST}.{loadName}";
        if (PlayerPrefs.HasKey(nombreLargo))
        {
            var json = PlayerPrefs.GetString(nombreLargo, "{}");
            JsonUtility.FromJsonOverwrite(json, configuration);
            configuration.name = loadName;
        }
    }

    [SerializeField]
    private ImageExplorer _imageExplorer;
    [SerializeField]
    MinMaxUIControl _hueControl;
    [SerializeField]
    MinMaxUIControl _satControl;
    [SerializeField]
    MinMaxUIControl _valControl;
    [SerializeField]
    Slider _simplifySlider;

    public bool _usarColorLineal = true;
    public bool _usarHSV = true;

    public event System.Action<string> AlGuardarConfiguracion;

    [SerializeField]
    private TestColorBlob _testColorBlob;
    [SerializeField]
    private Button _testButton;
    [SerializeField]
    private InputGuardarConfig _guardarButton;

    [SerializeField]
    private ColorBlobs _defaultColorBlobs;
    ColorBlobs _colorBlobTest;
    
    void Awake()
    {
        CVManager.AlCambiarImagen(AlCambiarImagen);
        CVManager.AlGenerarHSVMat(AlCambiarHSV);
        _imageExplorer.Texture = null;

        _hueControl.AlActualizar += AlCambiarParametros;
        _satControl.AlActualizar += AlCambiarParametros;
        _valControl.AlActualizar += AlCambiarParametros;
        _simplifySlider.onValueChanged.AddListener((val) => AlCambiarParametros(default(Vector2Int)));

        _testButton.onClick.AddListener(TestColorBlob);

        _guardarButton.OnSave += Guardar;

        if (_colorBlobTest == null)
            _colorBlobTest = ScriptableObject.Instantiate(_defaultColorBlobs);
    }

    IEnumerator Start()
    {
        yield return null;
        LayoutRebuilder.MarkLayoutForRebuild((RectTransform)transform);
    }

    public void Configurar(string config)
    {
        if (_colorBlobTest == null)
            _colorBlobTest = ScriptableObject.Instantiate(_defaultColorBlobs);

        if (config == null)
        {
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(_defaultColorBlobs), _colorBlobTest);
            _guardarButton.Text = string.Empty;
        }
        else
        {
            var nombreLargo = $"{CONFIG_BLOBS_LIST}.{config}";
            var json = PlayerPrefs.GetString(nombreLargo, "{}");
            JsonUtility.FromJsonOverwrite(json, _colorBlobTest);

            _hueControl.SetMinMaxWithoutNotify(_colorBlobTest.HueValido);
            _satControl.SetMinMaxWithoutNotify(_colorBlobTest.SaturacionValida);
            _valControl.SetMinMaxWithoutNotify(_colorBlobTest.BrilloValido);
            _simplifySlider.SetValueWithoutNotify(_colorBlobTest.SimplifyContour);

            AlCambiarParametros(Vector2Int.zero);

            _guardarButton.Text = config;
        }
    }

    public ColorBlobs GenerarScriptableObject(string nombre)
    {
        if (!string.IsNullOrEmpty(nombre))
        {
            var nombreLargo = $"{CONFIG_BLOBS_LIST}.{nombre}";
            if (PlayerPrefs.HasKey(nombreLargo))
            {
                var json = PlayerPrefs.GetString(nombreLargo, "{}");
                var resultado = ScriptableObject.Instantiate(_defaultColorBlobs);
                JsonUtility.FromJsonOverwrite(json, resultado);
                return resultado;
            }
        }
        return null;
    }

    private void OnEnable()
    {
        AlCambiarParametros(Vector2Int.zero);
    }

    private void Guardar(string nombre)
    {
        if (!string.IsNullOrEmpty(nombre))
        {
            var nombreLargo = $"{CONFIG_BLOBS_LIST}.{nombre}";
            if (!PlayerPrefs.HasKey(nombreLargo))
            {
                List<string> lista = ListaConfigBlobs;
                lista.Add(nombre);
                ListaConfigBlobs = lista;
            }

            PlayerPrefs.SetString(nombreLargo, JsonUtility.ToJson(_colorBlobTest));
        }

        AlGuardarConfiguracion?.Invoke(nombre);
    }

    private void AlCambiarImagen(Texture2D imagen)
    {
        if (_usarHSV || !imagen)
            return;

        _imageExplorer.Texture = imagen;
    }

    private void AlCambiarHSV(Mat hsvMat)
    {
        if (_usarHSV && hsvMat != null)
            _imageExplorer.Texture = OpenCvSharp.Unity.MatToTexture(hsvMat, _imageExplorer.Texture as Texture2D);
    }

    private void AlCambiarParametros(Vector2Int _)
    {
        var mat = _imageExplorer.Material;

        Color colMin = Color.white;
        Color colMax = Color.white;
        Vector4 invertir = Vector4.zero;

        invertir[0] = _hueControl.MinMaxValue.x > _hueControl.MinMaxValue.y ? 1 : 0;
        invertir[1] = _satControl.MinMaxValue.x > _satControl.MinMaxValue.y ? 1 : 0;
        invertir[2] = _valControl.MinMaxValue.x > _valControl.MinMaxValue.y ? 1 : 0;

        colMin[0] = invertir[0] == 1 ? _hueControl.MinMaxValue.y / 255f : _hueControl.MinMaxValue.x / 255f;
        colMax[0] = invertir[0] == 1 ? _hueControl.MinMaxValue.x / 255f : _hueControl.MinMaxValue.y / 255f;
        colMin[1] = invertir[1] == 1 ? _satControl.MinMaxValue.y / 255f : _satControl.MinMaxValue.x / 255f;
        colMax[1] = invertir[1] == 1 ? _satControl.MinMaxValue.x / 255f : _satControl.MinMaxValue.y / 255f;
        colMin[2] = invertir[2] == 1 ? _valControl.MinMaxValue.y / 255f : _valControl.MinMaxValue.x / 255f;
        colMax[2] = invertir[2] == 1 ? _valControl.MinMaxValue.x / 255f : _valControl.MinMaxValue.y / 255f;

        if (_usarColorLineal)
        {
            mat.SetColor("_HSVMin", colMin.linear);
            mat.SetColor("_HSVMax", colMax.linear);
        }
        else
        {
            mat.SetColor("_HSVMin", colMin);
            mat.SetColor("_HSVMax", colMax);
        }

        mat.SetVector("_Invertir", invertir);

        _colorBlobTest.HueValido = _hueControl.MinMaxValue;
        _colorBlobTest.SaturacionValida = _satControl.MinMaxValue;
        _colorBlobTest.BrilloValido = _valControl.MinMaxValue;
        _colorBlobTest.SimplifyContour = _simplifySlider.value;
    }

    private void TestColorBlob()
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (CVManager.HsvMat == null)
            return;

        _testColorBlob.gameObject.SetActive(true);
        _testColorBlob.Test(_hueControl.MinMaxValue, _satControl.MinMaxValue, _valControl.MinMaxValue);

        gameObject.SetActive(false);
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ColorBlobConfigurator))]
    private class ColorBlobConfiguratorEditor : Editor
    {
        [SerializeField]
        public static ColorBlobs _blobInst;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            foreach (var opcion in (List<string>)ListaConfigBlobs)
            {
                var nombreLargo = $"{CONFIG_BLOBS_LIST}.{opcion}";
                GUILayout.Label(nombreLargo);
                var json = PlayerPrefs.GetString(nombreLargo, "{}");
                EditorGUILayout.TextArea(json);
                if (GUILayout.Button("Generar"))
                {
                    if (_blobInst == null)
                        _blobInst = ScriptableObject.CreateInstance<ColorBlobs>();
                    JsonUtility.FromJsonOverwrite(json, _blobInst);
                    Selection.activeObject = _blobInst;
                }
            }
        }
    }
#endif
}
