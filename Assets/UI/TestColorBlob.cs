using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rect = UnityEngine.Rect;
using CvRect = OpenCvSharp.Rect;
using OpenCvSharp;
using UnityEngine.UI;
using TMPro;

public class TestColorBlob : MonoBehaviour
{
    [SerializeField]
    private ColorBlobs _defaultColorBlobs;

    [SerializeField]
    private ImageExplorer _imageExplorer;

    [SerializeField]
    private float _margenesDemo = 4f;

    ColorBlobs _colorBlobTest;

    Texture2D _resultadoBinarioTex2D;

    [SerializeField]
    private ColorBlobConfigurator _colorBlobConfig;
    [SerializeField]
    private Button _backButton;

    [SerializeField]
    private Transform _templateDemo;

    private List<RawImage> _demoPool = new();
    private Dictionary<RawImage, TMP_Text> _demoPoolToText = new();

    void Awake()
    {
        CVManager.AlGenerarHSVMat(Test);

        _backButton.onClick.AddListener(() =>
        {
            _colorBlobConfig.gameObject.SetActive(true);
            gameObject.SetActive(false);
        });
    }

    void OnDestroy()
    {
        if (_resultadoBinarioTex2D != null)
            Destroy(_resultadoBinarioTex2D);
    }

    public void Test(Vector2Int hueMinMax, Vector2Int satMinMax, Vector2Int valMinMax)
    {
        if (CVManager.HsvMat != null && !CVManager.HsvMat.IsDisposed)
        {
            if (_colorBlobTest == null)
                _colorBlobTest = ScriptableObject.Instantiate(_defaultColorBlobs);

            _colorBlobTest.HueValido = hueMinMax;
            _colorBlobTest.SaturacionValida = satMinMax;
            _colorBlobTest.BrilloValido = valMinMax;

            Test(CVManager.HsvMat);
        }
    }

    public void Test(Mat _hsvMat)
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (_colorBlobTest != null && _hsvMat != null && !_hsvMat.IsDisposed)
        {
            using (Mat resultadoBinario = new Mat())
            {
                _colorBlobTest.FromHueMat(CVManager.HsvMat, CVManager.TipoHue, resultadoBinario, out Point[][] contornos, out HierarchyIndex[] jerarquias);

                Cv2.CvtColor(resultadoBinario, resultadoBinario, ColorConversionCodes.GRAY2BGR);
                Cv2.DrawContours(resultadoBinario, contornos, -1, Scalar.Red);
                _resultadoBinarioTex2D = OpenCvSharp.Unity.MatToTexture(resultadoBinario, _resultadoBinarioTex2D);
                _imageExplorer.Texture = _resultadoBinarioTex2D;

                float matWidth = CVManager.HsvMat.Width;
                float matHeight = CVManager.HsvMat.Height;
                float matAspect = matHeight / matWidth;

                foreach (var demo in _demoPool)
                {
                    demo.texture = null;
                    demo.transform.parent.gameObject.SetActive(false);
                }

                for (int i = 0; i < contornos.Length; i++)
                {
                    if (contornos[i].Length <= 2) // una lina sin area ni nada muy complicado.. o un punto osea nada que ver
                        continue;

                    while (_demoPool.Count <= i)
                    {
                        var newDemo = Instantiate(_templateDemo, _templateDemo.parent).GetComponentInChildren<RawImage>(includeInactive: true);
                        _demoPool.Add(newDemo);
                        _demoPoolToText.Add(newDemo, newDemo.transform.parent.GetComponentInChildren<TMP_Text>(includeInactive: true));
                    }

                    _demoPool[i].transform.parent.gameObject.SetActive(true);

                    _demoPool[i].texture = _resultadoBinarioTex2D;
                    _demoPool[i].uvRect = CVManager.ConvertirBBoxAUVRect(Cv2.BoundingRect(contornos[i]), matWidth, matHeight, Vector4.one * _margenesDemo);

                    _demoPoolToText[_demoPool[i]].text = Cv2.ContourArea(contornos[i]).ToString("0.0");

                    var escala = Vector3.one;
                    escala.y = matAspect * _demoPool[i].uvRect.height / _demoPool[i].uvRect.width;
                    escala.x = _demoPool[i].uvRect.width / (matAspect * _demoPool[i].uvRect.height);
                    if (escala.y < escala.x)
                        escala.x = 1f;
                    else
                        escala.y = 1f;
                    _demoPool[i].transform.localScale = escala;
                }
            }
        }
    }
}
