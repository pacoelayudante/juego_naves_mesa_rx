using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Rect = UnityEngine.Rect;
using CvRect = OpenCvSharp.Rect;
using OpenCvSharp;

public class ManagerDePartida : MonoBehaviour
{
    public TokenDetector _tokenDetectorDefault;
    public PlasmarEscenario _plasmarEscenario;

    public GameObject rootEscenaGenerada;

    public RawImage _imagenMundo;

    [System.NonSerialized]
    public float escalaAcomodarTam;

    void Start()
    {
        if (!string.IsNullOrEmpty(DetectorTokensConfigurator.ConfigSeleccionada))
        {
            var configNames = DetectorTokensConfigurator.LoadConfigNames(DetectorTokensConfigurator.ConfigSeleccionada);
            if (configNames != null)
            {
                _tokenDetectorDefault = Instantiate(_tokenDetectorDefault);
                ColorBlobConfigurator.LoadConfiguation(_tokenDetectorDefault._blobsPurpura, configNames.jugA);
                ColorBlobConfigurator.LoadConfiguation(_tokenDetectorDefault._blobsAmarillos, configNames.jugB);
                ColorBlobConfigurator.LoadConfiguation(_tokenDetectorDefault._blobsFuxia, configNames.especiales);
                TemplatesConfigurator.LoadConfiguation(_tokenDetectorDefault._tokenTemplates, configNames.tokens);
                _tokenDetectorDefault.MinBlobsSqArea = configNames.minArea;
            }
        }

        CVManager.AlCambiarImagen((img) =>
        {
            if (img == null)
                return;
                
            escalaAcomodarTam = (_plasmarEscenario.expectedImageSize / Mathf.Max(CVManager.Imagen.width, CVManager.Imagen.height));
        
            _imagenMundo.texture = img;
            _imagenMundo.SetNativeSize();
            _imagenMundo.transform.localScale = Vector3.one * escalaAcomodarTam;
        }, ejecutarImediato: true);
    }

    void OnDestroy()
    {
        if (rootEscenaGenerada)
            Destroy(rootEscenaGenerada);
    }

    public void ProcesarMat(Mat hsvMat)
    {
        if (rootEscenaGenerada)
            Destroy(rootEscenaGenerada);

        rootEscenaGenerada = _plasmarEscenario.PrepararEscenario(hsvMat, CVManager.TipoHue, _tokenDetectorDefault);
    }
}
