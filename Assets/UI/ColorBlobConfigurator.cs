using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCvSharp;

[DisallowMultipleComponent]
public class ColorBlobConfigurator : MonoBehaviour
{
    private const string CONFIG_BLOBS_LIST = "CONFIG_BLOBS_LIST";

    public static StringListWrapper ListaConfigBlobs
    {
        get => JsonUtility.FromJson<StringListWrapper>(PlayerPrefs.GetString(CONFIG_BLOBS_LIST, "{}"));
        set => PlayerPrefs.SetString(CONFIG_BLOBS_LIST, JsonUtility.ToJson(value));
    }

    [SerializeField]
    private ImageExplorer _imageExplorer;
    [SerializeField]
    MinMaxUIControl _hueControl;
    [SerializeField]
    MinMaxUIControl _satControl;
    [SerializeField]
    MinMaxUIControl _valControl;

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

    private Color ColorFiltroMin => new Color32((byte)_hueControl.MinMaxValue.x, (byte)_satControl.MinMaxValue.x, (byte)_valControl.MinMaxValue.x, 255);
    private Color ColorFiltroMax => new Color32((byte)_hueControl.MinMaxValue.y, (byte)_satControl.MinMaxValue.y, (byte)_valControl.MinMaxValue.y, 255);

    void Awake()
    {
        CVManager.AlCambiarImagen(AlCambiarImagen);
        CVManager.AlGenerarHSVMat(AlCambiarHSV);
        _imageExplorer.Texture = null;

        _hueControl.AlActualizar += AlCambiarParametros;
        _satControl.AlActualizar += AlCambiarParametros;
        _valControl.AlActualizar += AlCambiarParametros;

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
            JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(nombreLargo, "{}"), _colorBlobTest);
            AlCambiarParametros(Vector2Int.zero);
            _guardarButton.Text = config;
        }
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
            _imageExplorer.Texture = OpenCvSharp.Unity.MatToTexture(hsvMat, (Texture2D)_imageExplorer.Texture);
    }

    private void AlCambiarParametros(Vector2Int _)
    {
        var mat = _imageExplorer.Material;
        if (_usarColorLineal)
        {
            mat.SetColor("_HSVMin", ColorFiltroMin.linear);
            mat.SetColor("_HSVMax", ColorFiltroMax.linear);
        }
        else
        {
            mat.SetColor("_HSVMin", ColorFiltroMin);
            mat.SetColor("_HSVMax", ColorFiltroMax);
        }

        _colorBlobTest.HueValido = _hueControl.MinMaxValue;
        _colorBlobTest.SaturacionValida = _satControl.MinMaxValue;
        _colorBlobTest.BrilloValido = _valControl.MinMaxValue;
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
}
